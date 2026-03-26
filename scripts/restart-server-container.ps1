param(
    [string]$ServiceName = "server-api"
)

$projectRoot = Split-Path -Path $PSScriptRoot -Parent
Set-Location $projectRoot

Write-Host "Сборка новой версии сервиса '$ServiceName'..." -ForegroundColor Green
docker compose build $ServiceName
if ($LASTEXITCODE -ne 0)
{
    Write-Host "Ошибка сборки сервиса '$ServiceName'." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Проверка состояния контейнера сервиса '$ServiceName'..." -ForegroundColor Green
$containerId = docker compose ps -q $ServiceName
if ($LASTEXITCODE -ne 0)
{
    Write-Host "Не удалось получить состояние контейнера '$ServiceName'." -ForegroundColor Red
    exit $LASTEXITCODE
}

if (-not [string]::IsNullOrWhiteSpace($containerId))
{
    Write-Host "Остановка текущего контейнера '$ServiceName'..." -ForegroundColor Yellow
    docker compose stop $ServiceName
    if ($LASTEXITCODE -ne 0)
    {
        Write-Host "Не удалось остановить контейнер '$ServiceName'." -ForegroundColor Red
        exit $LASTEXITCODE
    }
}
else
{
    Write-Host "Контейнер '$ServiceName' отсутствует. Будет создан новый." -ForegroundColor Yellow
}

Write-Host "Создание/запуск контейнера '$ServiceName' с новой версией..." -ForegroundColor Green
docker compose up -d --force-recreate $ServiceName
if ($LASTEXITCODE -ne 0)
{
    Write-Host "Не удалось запустить контейнер '$ServiceName'." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Готово: контейнер '$ServiceName' запущен с новой версией." -ForegroundColor Green
