param(
    [ValidateSet("Gateway", "StressTest")]
    [string]$Scenario = "Gateway",

    [ValidateSet("k6", "JMeter")]
    [string]$Tool = "k6",

    [string]$Evaluator = $env:USERNAME,
    [string]$BaseUrl,
    [switch]$SkipBuild,
    [switch]$SkipDocker,
    [switch]$TemplateOnly
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$evaluationsDir = Join-Path $PSScriptRoot "evaluations"
$resultsDir = Join-Path (Join-Path $PSScriptRoot "load-tests") "results"
$templatePath = Join-Path (Join-Path $PSScriptRoot "load-tests") "EVALUATION-TEMPLATE.md"

New-Item -ItemType Directory -Force -Path $evaluationsDir | Out-Null
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

function Get-NextEvaluationId {
    $datePrefix = "RUN-$(Get-Date -Format 'yyyy-MM-dd')"
    $existing = Get-ChildItem -Path $evaluationsDir -Filter "$datePrefix-*.md" -ErrorAction SilentlyContinue
    $next = if ($existing) { ($existing.Count + 1) } else { 1 }
    return "{0}-{1:D2}" -f $datePrefix, $next
}

function Get-CommandVersion {
    param([string]$Command, [string[]]$VersionArgs = @("--version"))
    try {
        $output = & $Command @VersionArgs 2>&1 | Select-Object -First 1
        return ([string]$output).Trim()
    }
    catch {
        return "_(not installed)_"
    }
}

function Set-Placeholder {
    param(
        [string]$Content,
        [hashtable]$Values
    )
    foreach ($key in $Values.Keys) {
        $token = "{{$key}}"
        $value = [string]$Values[$key]
        $Content = $Content.Replace($token, $value)
    }
    return $Content
}

function Get-MetricValue {
    param($Metrics, [string]$Name, [string]$ValueKey = "value")
    if (-not $Metrics) { return $null }
    $metric = $Metrics.$Name
    if (-not $metric) { return $null }
    if ($metric.values) {
        if ($metric.values.PSObject.Properties.Name -contains $ValueKey) {
            return $metric.values.$ValueKey
        }
        if ($ValueKey -eq "value" -and $metric.values.PSObject.Properties.Name -contains "rate") {
            return $metric.values.rate
        }
        if ($ValueKey -eq "value" -and $metric.values.PSObject.Properties.Name -contains "count") {
            return $metric.values.count
        }
        if ($metric.values.PSObject.Properties.Name -contains $ValueKey) {
            return $metric.values.$ValueKey
        }
    }
    return $null
}

function Format-Number {
    param($Value, [int]$Decimals = 2, [string]$Suffix = "")
    if ($null -eq $Value -or $Value -eq "") { return "_(manual)_" }
    if ($Value -is [double] -or $Value -is [decimal] -or $Value -is [float]) {
        return ("{0:F$Decimals}{1}" -f $Value, $Suffix)
    }
    return "$Value$Suffix"
}

function Format-RatePercent {
    param($Rate)
    if ($null -eq $Rate) { return "_(manual)_" }
    return Format-Number ($Rate * 100) 2 "%"
}

function Parse-DiagnosticsLog {
    param(
        [string]$LogPath,
        [datetime]$TestStartUtc
    )

    $pattern = 'Posture:\s*(?<posture>\w+).*?Current RPS:\s*(?<current>[\d.]+).*?Forecasted 60s RPS:\s*(?<forecast>[\d.]+).*?Throttled Requests:\s*(?<throttled>\d+).*?P99 Internal Overhead:\s*(?<p99>[\d.]+)ms'
    $entries = @()

    if (-not (Test-Path $LogPath)) {
        return @{
            Snippet = "[PHASE 1 - Baseline]`n_(no diagnostics captured - check docker logs manually)_"
            Baseline = @{}
            Spike = @{}
            Cooldown = @{}
            TimeToCritical = "_(manual)_"
            TimeToNominal = "_(manual)_"
        }
    }

    foreach ($line in Get-Content $LogPath) {
        if ($line -notmatch $pattern) { continue }

        $timestamp = $null
        if ($line -match '(?<ts>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}') {
            $timestamp = [datetime]::Parse($Matches.ts, $null, [Globalization.DateTimeStyles]::AssumeUniversal)
        }
        elseif ($line -match '(?<ts>\d{2}:\d{2}:\d{2})') {
            $timestamp = $TestStartUtc.Date.Add([TimeSpan]::Parse($Matches.ts))
        }

        $elapsed = if ($timestamp) { ($timestamp.ToUniversalTime() - $TestStartUtc).TotalSeconds } else { $null }

        $entries += [pscustomobject]@{
            Line = $line.Trim()
            Posture = $Matches.posture
            CurrentRps = [double]$Matches.current
            ForecastRps = [double]$Matches.forecast
            Throttled = [int]$Matches.throttled
            P99Ms = [double]$Matches.p99
            ElapsedSec = $elapsed
        }
    }

    if ($entries.Count -eq 0) {
        return @{
            Snippet = "_(no posture lines matched - open diagnostics log manually)_"
            Baseline = @{}
            Spike = @{}
            Cooldown = @{}
            TimeToCritical = "_(manual)_"
            TimeToNominal = "_(manual)_"
        }
    }

    $baselineEntries = $entries | Where-Object { $null -eq $_.ElapsedSec -or $_.ElapsedSec -lt 30 }
    $spikeEntries = $entries | Where-Object { $null -ne $_.ElapsedSec -and $_.ElapsedSec -ge 30 -and $_.ElapsedSec -lt 45 }
    $cooldownEntries = $entries | Where-Object { $null -ne $_.ElapsedSec -and $_.ElapsedSec -ge 45 }

    if ($baselineEntries.Count -eq 0 -or $spikeEntries.Count -eq 0 -or $cooldownEntries.Count -eq 0) {
        $third = [math]::Max(1, [math]::Floor($entries.Count / 3))
        $baselineEntries = $entries[0..([math]::Min($third - 1, $entries.Count - 1))]
        $spikeEntries = $entries[$third..([math]::Min((2 * $third) - 1, $entries.Count - 1))]
        $cooldownEntries = $entries[([math]::Min(2 * $third, $entries.Count - 1))..($entries.Count - 1)]
    }

    function Get-PhaseSummary($phaseEntries) {
        if (-not $phaseEntries -or $phaseEntries.Count -eq 0) {
            return @{
                DominantPosture = "_(manual)_"
                PeakRps = "_(manual)_"
                PeakForecast = "_(manual)_"
                Throttled = "_(manual)_"
                P99 = "_(manual)_"
                SampleLine = "_(no sample)_"
            }
        }

        $dominant = ($phaseEntries | Group-Object Posture | Sort-Object Count -Descending | Select-Object -First 1).Name
        $last = $phaseEntries[-1]
        return @{
            DominantPosture = $dominant
            PeakRps = Format-Number ($phaseEntries | Measure-Object CurrentRps -Maximum).Maximum 0
            PeakForecast = Format-Number ($phaseEntries | Measure-Object ForecastRps -Maximum).Maximum 0
            Throttled = $last.Throttled
            P99 = Format-Number ($phaseEntries | Measure-Object P99Ms -Maximum).Maximum 2
            SampleLine = $phaseEntries[[math]::Floor($phaseEntries.Count / 2)].Line
        }
    }

    $baseline = Get-PhaseSummary $baselineEntries
    $spike = Get-PhaseSummary $spikeEntries
    $cooldown = Get-PhaseSummary $cooldownEntries

    $criticalEntry = $spikeEntries | Where-Object { $_.Posture -match 'Critical' } | Select-Object -First 1
    $timeToCritical = if ($criticalEntry -and $null -ne $criticalEntry.ElapsedSec) {
        Format-Number ($criticalEntry.ElapsedSec - 30) 1 "s"
    }
    else { "_(manual)_" }

    $nominalEntry = $cooldownEntries | Where-Object { $_.Posture -match 'Nominal' } | Select-Object -First 1
    $timeToNominal = if ($nominalEntry -and $null -ne $nominalEntry.ElapsedSec) {
        Format-Number ($nominalEntry.ElapsedSec - 45) 1 "s"
    }
    else { "_(manual)_" }

    $snippet = @(
        "[PHASE 1 - Baseline]",
        $baseline.SampleLine,
        "",
        "[PHASE 2 - Spike]",
        $spike.SampleLine,
        "",
        "[PHASE 3 - Cool-down]",
        $cooldown.SampleLine
    ) -join "`n"

    return @{
        Snippet = $snippet
        Baseline = $baseline
        Spike = $spike
        Cooldown = $cooldown
        TimeToCritical = $timeToCritical
        TimeToNominal = $timeToNominal
    }
}

function Get-HypothesisResults {
    param(
        $Diagnostics,
        $K6,
        [string]$CriticalMetric,
        [string]$NonCriticalMetric
    )

    $h1 = if ($Diagnostics.Spike.DominantPosture -match 'Alert|Critical') { "Pass" } else { "Partial" }
    $h1Evidence = "Spike posture: $($Diagnostics.Spike.DominantPosture); peak forecast RPS: $($Diagnostics.Spike.PeakForecast)"

    $criticalRate = Get-MetricValue $K6 $CriticalMetric
    $h2 = if ($null -ne $criticalRate -and $criticalRate -ge 0.90) { "Pass" }
    elseif ($null -ne $criticalRate -and $criticalRate -ge 0.75) { "Partial" }
    else { "Fail" }
    $h2Evidence = if ($null -ne $criticalRate) { "$CriticalMetric = $(Format-RatePercent $criticalRate)" } else { "_(manual)_" }

    $throttled = Get-MetricValue $K6 "throttled_429" "count"
    $h3 = if ($throttled -gt 0) { "Pass" } else { "Partial" }
    $h3Evidence = "throttled_429 count = $(if ($null -ne $throttled) { $throttled } else { '_(manual)_' })"

    $h4 = if ($Diagnostics.Cooldown.DominantPosture -match 'Nominal') { "Pass" } else { "Partial" }
    $h4Evidence = "Cool-down posture: $($Diagnostics.Cooldown.DominantPosture)"

    $p99Overhead = $Diagnostics.Spike.P99
    $h5 = if ($p99Overhead -match '^[\d.]+$' -and [double]$p99Overhead -lt 1.0) { "Pass" }
    elseif ($p99Overhead -match '^[\d.]+$' -and [double]$p99Overhead -lt 5.0) { "Partial" }
    else { "_(manual)_" }
    $h5Evidence = "Spike P99 internal overhead: $p99Overhead ms"

    return @{
        H1_RESULT = $h1; H1_EVIDENCE = $h1Evidence
        H2_RESULT = $h2; H2_EVIDENCE = $h2Evidence
        H3_RESULT = $h3; H3_EVIDENCE = $h3Evidence
        H4_RESULT = $h4; H4_EVIDENCE = $h4Evidence
        H5_RESULT = $h5; H5_EVIDENCE = $h5Evidence
    }
}

$config = switch ($Scenario) {
    "Gateway" {
        [pscustomobject]@{
            Label = "ApiGateway + microservices"
            ComposeFile = "docker-compose.yml"
            ServiceName = "apigateway"
            HealthUrl = "http://localhost:5000/health"
            DefaultBaseUrl = "http://localhost:5000"
            K6Script = "load-tests/k6/gateway-microservices-stress.js"
            CriticalMetric = "orders_ok"
            NonCriticalMetric = "products_ok"
        }
    }
    "StressTest" {
        [pscustomobject]@{
            Label = "StressTest.Api (isolated middleware)"
            ComposeFile = "docker-compose.loadtest.yml"
            ServiceName = "stress-api"
            HealthUrl = "http://localhost:5000/health"
            DefaultBaseUrl = "http://localhost:5000"
            K6Script = "load-tests/k6/predictive-middleware-stress.js"
            CriticalMetric = "checkout_ok"
            NonCriticalMetric = "promotions_ok"
        }
    }
}

if (-not $BaseUrl) { $BaseUrl = $config.DefaultBaseUrl }

$evaluationId = Get-NextEvaluationId
$evaluationFile = Join-Path $evaluationsDir "$evaluationId.md"
$k6SummaryFile = Join-Path $resultsDir "$evaluationId-k6-summary.json"
$diagnosticsLog = Join-Path $evaluationsDir "$evaluationId-diagnostics.log"
$jmeterResults = Join-Path $PSScriptRoot "results.jtl"
$jmeterReport = Join-Path $PSScriptRoot "ReportOutput"

try {
    $gitCommit = (git -C $PSScriptRoot rev-parse --short HEAD 2>$null).Trim()
    if (-not $gitCommit) { $gitCommit = "_(unknown)_" }
}
catch {
    $gitCommit = "_(unknown)_"
}

$placeholders = @{
    EVALUATION_FILE = "evaluations/$evaluationId.md"
    K6_SUMMARY_FILE = if ($Tool -eq "k6") { "load-tests/results/$evaluationId-k6-summary.json" } else { "_(n/a)_" }
    DIAGNOSTICS_LOG_FILE = "evaluations/$evaluationId-diagnostics.log"
    JMETER_RESULTS_FILE = if ($Tool -eq "JMeter") { "results.jtl" } else { "_(n/a)_" }
    JMETER_REPORT_DIR = if ($Tool -eq "JMeter") { "ReportOutput/index.html" } else { "_(n/a)_" }
    EVALUATION_ID = $evaluationId
    DATETIME_UTC = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
    EVALUATOR = $Evaluator
    SCENARIO = $config.Label
    TOOL = $Tool
    SCRIPT_PATH = if ($Tool -eq "k6") { $config.K6Script } else { "LoadTestPlan.jmx" }
    GIT_COMMIT = $gitCommit
    HOST_OS = [System.Environment]::OSVersion.VersionString
    DOTNET_VERSION = Get-CommandVersion "dotnet"
    TOOL_VERSION = if ($Tool -eq "k6") { Get-CommandVersion "k6" "version" } else { Get-CommandVersion "jmeter" }
    K6_CRITICAL_METRIC = $config.CriticalMetric
    K6_NONCRITICAL_METRIC = $config.NonCriticalMetric
    DIAGNOSTICS_SNIPPET = "_(run in progress)_"
    DOMINANT_POSTURE_BASELINE = "_(pending)_"
    DOMINANT_POSTURE_SPIKE = "_(pending)_"
    DOMINANT_POSTURE_COOLDOWN = "_(pending)_"
    PEAK_RPS_BASELINE = "_(pending)_"
    PEAK_RPS_SPIKE = "_(pending)_"
    PEAK_RPS_COOLDOWN = "_(pending)_"
    PEAK_FORECAST_BASELINE = "_(pending)_"
    PEAK_FORECAST_SPIKE = "_(pending)_"
    PEAK_FORECAST_COOLDOWN = "_(pending)_"
    THROTTLED_BASELINE = "_(pending)_"
    THROTTLED_SPIKE = "_(pending)_"
    THROTTLED_COOLDOWN = "_(pending)_"
    P99_OVERHEAD_BASELINE = "_(pending)_"
    P99_OVERHEAD_SPIKE = "_(pending)_"
    P99_OVERHEAD_COOLDOWN = "_(pending)_"
    TIME_TO_CRITICAL = "_(pending)_"
    TIME_TO_NOMINAL = "_(pending)_"
    K6_TOTAL_REQUESTS = "_(pending)_"
    K6_HTTP_REQ_FAILED_RATE = "_(pending)_"
    K6_P50_MS = "_(pending)_"
    K6_P95_MS = "_(pending)_"
    K6_P99_MS = "_(pending)_"
    K6_THROTTLED_429 = "_(pending)_"
    K6_CRITICAL_OK_RATE = "_(pending)_"
    K6_NONCRITICAL_OK_RATE = "_(pending)_"
    H1_RESULT = "_(pending)_"; H1_EVIDENCE = "_(pending)_"
    H2_RESULT = "_(pending)_"; H2_EVIDENCE = "_(pending)_"
    H3_RESULT = "_(pending)_"; H3_EVIDENCE = "_(pending)_"
    H4_RESULT = "_(pending)_"; H4_EVIDENCE = "_(pending)_"
    H5_RESULT = "_(pending)_"; H5_EVIDENCE = "_(pending)_"
}

$template = Get-Content $templatePath -Raw -Encoding UTF8
$initialDoc = Set-Placeholder $template $placeholders
[System.IO.File]::WriteAllText($evaluationFile, $initialDoc, [System.Text.UTF8Encoding]::new($false))

Write-Host "Created evaluation record: $evaluationFile"

if ($TemplateOnly) {
    Write-Host "TemplateOnly specified - skipping infrastructure and load test."
    $placeholders.DIAGNOSTICS_SNIPPET = "_(test not run - TemplateOnly mode)_"
    foreach ($key in @(
        'DOMINANT_POSTURE_BASELINE','DOMINANT_POSTURE_SPIKE','DOMINANT_POSTURE_COOLDOWN',
        'PEAK_RPS_BASELINE','PEAK_RPS_SPIKE','PEAK_RPS_COOLDOWN',
        'PEAK_FORECAST_BASELINE','PEAK_FORECAST_SPIKE','PEAK_FORECAST_COOLDOWN',
        'THROTTLED_BASELINE','THROTTLED_SPIKE','THROTTLED_COOLDOWN',
        'P99_OVERHEAD_BASELINE','P99_OVERHEAD_SPIKE','P99_OVERHEAD_COOLDOWN',
        'TIME_TO_CRITICAL','TIME_TO_NOMINAL',
        'K6_TOTAL_REQUESTS','K6_HTTP_REQ_FAILED_RATE','K6_P50_MS','K6_P95_MS','K6_P99_MS',
        'K6_THROTTLED_429','K6_CRITICAL_OK_RATE','K6_NONCRITICAL_OK_RATE',
        'H1_RESULT','H1_EVIDENCE','H2_RESULT','H2_EVIDENCE','H3_RESULT','H3_EVIDENCE',
        'H4_RESULT','H4_EVIDENCE','H5_RESULT','H5_EVIDENCE'
    )) { $placeholders[$key] = "_(not run)_" }
    $templateOnlyDoc = Set-Placeholder $template $placeholders
    [System.IO.File]::WriteAllText($evaluationFile, $templateOnlyDoc, [System.Text.UTF8Encoding]::new($false))
    Invoke-Item $evaluationFile
    exit 0
}

if (-not $SkipDocker) {
    Write-Host "Starting Docker stack ($($config.ComposeFile))..."
    $composeArgs = @("compose", "-f", $config.ComposeFile, "up", "-d")
    if (-not $SkipBuild) { $composeArgs += "--build" }
    & docker @composeArgs
}

Write-Host "Waiting for health endpoint $($config.HealthUrl)..."
$healthy = $false
for ($attempt = 1; $attempt -le 45; $attempt++) {
    try {
        Invoke-WebRequest -Uri $config.HealthUrl -UseBasicParsing -TimeoutSec 3 | Out-Null
        $healthy = $true
        break
    }
    catch {
        Start-Sleep -Seconds 2
    }
}
if (-not $healthy) {
    throw "Service did not become healthy at $($config.HealthUrl)."
}

$testStartUtc = (Get-Date).ToUniversalTime()
$logJob = $null
$k6Metrics = $null

if ($Tool -eq "k6") {
    if (-not (Get-Command k6 -ErrorAction SilentlyContinue)) {
        throw "k6 is not installed or not on PATH. Install from https://k6.io/docs/get-started/installation/"
    }

    Write-Host "Capturing diagnostics from container '$($config.ServiceName)'..."
    $logJob = Start-Job -ScriptBlock {
        param($Root, $ComposeFile, $Service, $LogPath)
        Set-Location $Root
        docker compose -f $ComposeFile logs -f $Service 2>&1 |
            Tee-Object -FilePath $LogPath |
            Out-Null
    } -ArgumentList $PSScriptRoot, $config.ComposeFile, $config.ServiceName, $diagnosticsLog

    Start-Sleep -Seconds 2

    Write-Host "Running k6 ($($config.K6Script)) against $BaseUrl ..."
    & k6 run -e "BASE_URL=$BaseUrl" -e "K6_TEST_RUN=$evaluationId" `
        --summary-export $k6SummaryFile `
        $config.K6Script

    if ($logJob) {
        Stop-Job $logJob -ErrorAction SilentlyContinue
        Remove-Job $logJob -Force -ErrorAction SilentlyContinue
    }

    if (-not (Test-Path $diagnosticsLog) -or (Get-Item $diagnosticsLog).Length -eq 0) {
        Write-Host "Diagnostics capture empty - fetching recent container logs..."
        docker compose -f $config.ComposeFile logs --since 10m $config.ServiceName 2>&1 |
            Set-Content -Path $diagnosticsLog -Encoding utf8
    }

    if (Test-Path $k6SummaryFile) {
        $k6Summary = Get-Content $k6SummaryFile -Raw | ConvertFrom-Json
        $k6Metrics = $k6Summary.metrics
        $placeholders.K6_TOTAL_REQUESTS = Get-MetricValue $k6Metrics "http_reqs" "count"
        $placeholders.K6_HTTP_REQ_FAILED_RATE = Format-RatePercent (Get-MetricValue $k6Metrics "http_req_failed" "rate")
        $placeholders.K6_P50_MS = Format-Number (Get-MetricValue $k6Metrics "http_req_duration" "med") 2
        $placeholders.K6_P95_MS = Format-Number (Get-MetricValue $k6Metrics "http_req_duration" "p(95)") 2
        $placeholders.K6_P99_MS = Format-Number (Get-MetricValue $k6Metrics "http_req_duration" "p(99)") 2
        $placeholders.K6_THROTTLED_429 = Get-MetricValue $k6Metrics "throttled_429" "count"
        $placeholders.K6_CRITICAL_OK_RATE = Format-RatePercent (Get-MetricValue $k6Metrics $config.CriticalMetric)
        $placeholders.K6_NONCRITICAL_OK_RATE = Format-RatePercent (Get-MetricValue $k6Metrics $config.NonCriticalMetric)
    }
}
else {
    Write-Host "Running JMeter via run-loadtest.ps1..."
    $jmeterArgs = @{
        ComposeFile = $config.ComposeFile
    }
    if ($SkipBuild) { $jmeterArgs.SkipBuild = $true }
    & (Join-Path $PSScriptRoot "run-loadtest.ps1") @jmeterArgs

    if (-not (Test-Path $diagnosticsLog)) {
        docker compose -f $config.ComposeFile logs --since 10m $config.ServiceName 2>&1 |
            Set-Content -Path $diagnosticsLog -Encoding utf8
    }
}

$diagnostics = Parse-DiagnosticsLog -LogPath $diagnosticsLog -TestStartUtc $testStartUtc
$placeholders.DIAGNOSTICS_SNIPPET = $diagnostics.Snippet
$placeholders.DOMINANT_POSTURE_BASELINE = $diagnostics.Baseline.DominantPosture
$placeholders.DOMINANT_POSTURE_SPIKE = $diagnostics.Spike.DominantPosture
$placeholders.DOMINANT_POSTURE_COOLDOWN = $diagnostics.Cooldown.DominantPosture
$placeholders.PEAK_RPS_BASELINE = $diagnostics.Baseline.PeakRps
$placeholders.PEAK_RPS_SPIKE = $diagnostics.Spike.PeakRps
$placeholders.PEAK_RPS_COOLDOWN = $diagnostics.Cooldown.PeakRps
$placeholders.PEAK_FORECAST_BASELINE = $diagnostics.Baseline.PeakForecast
$placeholders.PEAK_FORECAST_SPIKE = $diagnostics.Spike.PeakForecast
$placeholders.PEAK_FORECAST_COOLDOWN = $diagnostics.Cooldown.PeakForecast
$placeholders.THROTTLED_BASELINE = $diagnostics.Baseline.Throttled
$placeholders.THROTTLED_SPIKE = $diagnostics.Spike.Throttled
$placeholders.THROTTLED_COOLDOWN = $diagnostics.Cooldown.Throttled
$placeholders.P99_OVERHEAD_BASELINE = $diagnostics.Baseline.P99
$placeholders.P99_OVERHEAD_SPIKE = $diagnostics.Spike.P99
$placeholders.P99_OVERHEAD_COOLDOWN = $diagnostics.Cooldown.P99
$placeholders.TIME_TO_CRITICAL = $diagnostics.TimeToCritical
$placeholders.TIME_TO_NOMINAL = $diagnostics.TimeToNominal

$hypothesis = Get-HypothesisResults -Diagnostics $diagnostics -K6 $k6Metrics `
    -CriticalMetric $config.CriticalMetric -NonCriticalMetric $config.NonCriticalMetric
foreach ($key in $hypothesis.Keys) {
    $placeholders[$key] = $hypothesis[$key]
}

$finalDoc = Set-Placeholder $template $placeholders
[System.IO.File]::WriteAllText($evaluationFile, $finalDoc, [System.Text.UTF8Encoding]::new($false))

Write-Host ""
Write-Host "Evaluation complete."
Write-Host "  Record:      $evaluationFile"
Write-Host "  Diagnostics: $diagnosticsLog"
if ($Tool -eq "k6") { Write-Host "  k6 summary:  $k6SummaryFile" }
if ($Tool -eq "JMeter") { Write-Host "  JMeter HTML: $jmeterReport/index.html" }
Write-Host ""
Write-Host "Review sections 9-10 and any _(manual)_ fields, then use this record in your thesis."

Invoke-Item $evaluationFile
