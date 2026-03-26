param(
    [int]$DefaultPort = 8090
)

$projectRoot = Split-Path -Path $PSScriptRoot -Parent
$agentProjectPath = Join-Path $projectRoot "Host.Agent/Host.Agent.csproj"

$portInput = Read-Host "Введите порт для Host.Agent (Enter = $DefaultPort)"

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

Write-Host "Запуск Host.Agent на порту $port..." -ForegroundColor Green
dotnet run --project $agentProjectPath -- --port $port
