# run-local.ps1
# Set environment to Development so all APIs pick up appsettings.Development.json
$env:ASPNETCORE_ENVIRONMENT = "Development"
$ErrorActionPreference = "Continue"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  MIT Auto-Scaling Stack Local Orchestrator" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Ensure logs directory exists
$logDir = "$PSScriptRoot/logs"
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir | Out-Null
}

# 1. Start LocalDB
Write-Host "Starting MS SQL Server LocalDB..." -ForegroundColor Yellow
sqllocaldb start MSSQLLocalDB

# 2. Start Redis Server
$redisExe = "C:\Users\BUYPC COMPUTERS\Documents\MITProject\redis-server\redis-server.exe"
if (-not (Test-Path $redisExe)) {
    Write-Host "Error: Redis executable not found at $redisExe" -ForegroundColor Red
    exit 1
}
Write-Host "Starting Redis Server..." -ForegroundColor Yellow
$redisProc = Start-Process $redisExe -ArgumentList "--port 6379" -RedirectStandardOutput "$logDir/redis.log" -RedirectStandardError "$logDir/redis-error.log" -PassThru -WindowStyle Minimized

# List of .NET projects to start
$projects = @(
    @{ Name = "UserService"; Path = "UserService/Api/UserService.Api.csproj" },
    @{ Name = "ProductService"; Path = "ProductService/Api/ProductService.Api.csproj" },
    @{ Name = "OrderService"; Path = "OrderService/Api/OrderService.Api.csproj" },
    @{ Name = "MonitoringService"; Path = "MonitoringService/Api/MonitoringService.Api.csproj" },
    @{ Name = "PredictionService"; Path = "PredictionService/Api/PredictionService.Api.csproj" },
    @{ Name = "ApiGateway"; Path = "ApiGateway/ApiGateway.csproj" }
)

$processes = @()

# 3. Start .NET backend services
Write-Host "Starting backend services..." -ForegroundColor Yellow
foreach ($proj in $projects) {
    Write-Host "Starting $($proj.Name)..." -ForegroundColor Yellow
    $proc = Start-Process dotnet -ArgumentList "run --project $($proj.Path)" -RedirectStandardOutput "$logDir/$($proj.Name).log" -RedirectStandardError "$logDir/$($proj.Name)-error.log" -PassThru -WindowStyle Minimized
    $processes += @{ Name = $proj.Name; Process = $proc }
}

# 4. Start React frontend
Write-Host "Checking frontend dependencies..." -ForegroundColor Yellow
$frontendDir = "C:\Users\BUYPC COMPUTERS\Documents\MITProject\frontend"
if (-not (Test-Path "$frontendDir\node_modules")) {
    Write-Host "Installing frontend dependencies (npm install)..." -ForegroundColor Yellow
    Start-Process npm -ArgumentList "install" -WorkingDirectory $frontendDir -Wait -NoNewWindow
}

Write-Host "Starting React Frontend (Vite)..." -ForegroundColor Yellow
$frontendProc = Start-Process npm -ArgumentList "run dev" -WorkingDirectory $frontendDir -RedirectStandardOutput "$logDir/frontend.log" -RedirectStandardError "$logDir/frontend-error.log" -PassThru -WindowStyle Minimized

Write-Host "=============================================" -ForegroundColor Green
Write-Host "  Stack has been started in the background!" -ForegroundColor Green
Write-Host "  Storefront: http://localhost:5173" -ForegroundColor Green
Write-Host "  API Gateway: http://localhost:5000" -ForegroundColor Green
Write-Host "  To stop all services, run .\stop-local.ps1" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Green

Write-Host "Keeping stack alive. The background task is blocking. Stop this task to clean up." -ForegroundColor Cyan
try {
    while ($true) {
        Start-Sleep -Seconds 5
    }
}
finally {
    Write-Host "Stopping all services..." -ForegroundColor Red
    if ($frontendProc) { Stop-Process -Id $frontendProc.Id -Force -ErrorAction SilentlyContinue }
    if ($redisProc) { Stop-Process -Id $redisProc.Id -Force -ErrorAction SilentlyContinue }
    foreach ($p in $processes) {
        Write-Host "Stopping $($p.Name)..." -ForegroundColor Red
        Stop-Process -Id $p.Process.Id -Force -ErrorAction SilentlyContinue
    }
}
