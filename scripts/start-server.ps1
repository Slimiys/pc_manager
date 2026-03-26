param(
    [int]$DefaultPort = 8080
)

$projectRoot = Split-Path -Path $PSScriptRoot -Parent
$serverProjectPath = Join-Path $projectRoot "Server.Api/Server.Api.csproj"

$portInput = Read-Host "Введите порт для Server.Api (Enter = $DefaultPort)"

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

Write-Host "Запуск Server.Api на порту $port..." -ForegroundColor Green
dotnet run --project $serverProjectPath -- --port $port
