param(
    [int]$DefaultPort = 8787,
    [switch]$Release
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Path $PSScriptRoot -Parent
$toastProjectPath = Join-Path $projectRoot "Client.ToastListener\Client.ToastListener.Desktop\Client.ToastListener.Desktop.csproj"

$configuration = if ($Release) { "Release" } else { "Debug" }

Write-Host "Сборка Client.ToastListener.Desktop ($configuration)..." -ForegroundColor Cyan
& dotnet build $toastProjectPath -c $configuration
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$portInput = Read-Host "Введите порт HTTP для Toast Listener (Enter = $DefaultPort)"

$port = $DefaultPort
if (-not [string]::IsNullOrWhiteSpace($portInput)) {
    if ($portInput -match '^\d+$') {
        $parsedPort = [int]$portInput
        if ($parsedPort -ge 1 -and $parsedPort -le 65535) {
            $port = $parsedPort
        }
        else {
            Write-Host "Некорректный порт '$portInput'. Используется порт по умолчанию $DefaultPort." -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Некорректный порт '$portInput'. Используется порт по умолчанию $DefaultPort." -ForegroundColor Yellow
    }
}

$env:ToastListener__HttpPort = "$port"

Write-Host "Запуск Toast Listener на порту $port..." -ForegroundColor Green
& dotnet run --project $toastProjectPath -c $configuration --no-build
