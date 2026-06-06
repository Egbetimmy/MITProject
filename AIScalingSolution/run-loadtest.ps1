param(
    [string]$ComposeFile = "docker-compose.loadtest.yml",
    [string]$JmxFile = "LoadTestPlan.jmx",
    [string]$ResultsFile = "results.jtl",
    [string]$ReportDir = "ReportOutput",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$composeArgs = @("compose", "-f", $ComposeFile, "up", "-d")
if (-not $SkipBuild) {
    $composeArgs += "--build"
}

Write-Host "Starting Redis and StressTest API containers..."
& docker @composeArgs

Write-Host "Waiting for StressTest API health endpoint..."
$healthy = $false
for ($attempt = 1; $attempt -le 30; $attempt++) {
    try {
        Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 2 | Out-Null
        $healthy = $true
        break
    }
    catch {
        Start-Sleep -Seconds 2
    }
}

if (-not $healthy) {
    throw "StressTest API did not become healthy at http://localhost:5000/health."
}

if (Test-Path $ResultsFile) {
    Remove-Item $ResultsFile -Force
}

if (Test-Path $ReportDir) {
    Remove-Item $ReportDir -Recurse -Force
}

$logCommand = "cd `"$PSScriptRoot`"; docker compose -f `"$ComposeFile`" logs -f stress-api"
Start-Process powershell -ArgumentList "-NoExit", "-Command", $logCommand | Out-Null

Write-Host "Running JMeter non-GUI load simulation..."
& jmeter -n -t $JmxFile -l $ResultsFile -e -o "./$ReportDir"

Write-Host "JMeter run complete."
Write-Host "Results file: $ResultsFile"
Write-Host "HTML report: $ReportDir/index.html"
