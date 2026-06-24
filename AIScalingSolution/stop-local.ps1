# stop-local.ps1
Write-Host "=============================================" -ForegroundColor Red
Write-Host "  Stopping MIT Auto-Scaling Stack Services" -ForegroundColor Red
Write-Host "=============================================" -ForegroundColor Red

# Stop Redis
Write-Host "Stopping Redis Server..." -ForegroundColor Yellow
Stop-Process -Name "redis-server" -Force -ErrorAction SilentlyContinue

# Stop Node/Vite (frontend)
Write-Host "Stopping Frontend process..." -ForegroundColor Yellow
Stop-Process -Name "node" -Force -ErrorAction SilentlyContinue

# Stop .NET backend APIs
Write-Host "Stopping .NET API services..." -ForegroundColor Yellow
# We target processes running the built executables
$dotnetProcesses = @("UserService.Api", "ProductService.Api", "OrderService.Api", "MonitoringService.Api", "PredictionService.Api", "ApiGateway")
foreach ($proc in $dotnetProcesses) {
    Stop-Process -Name $proc -Force -ErrorAction SilentlyContinue
}

# Also stop any general 'dotnet' processes launched by the run script
Stop-Process -Name "dotnet" -Force -ErrorAction SilentlyContinue

Write-Host "All stack services stopped successfully." -ForegroundColor Green
