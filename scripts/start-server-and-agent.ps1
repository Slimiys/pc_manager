<#
.SYNOPSIS
    Запускает Host.Agent и Server.Api в отдельных окнах PowerShell.

.DESCRIPTION
    По умолчанию: агент на порту 8090, API на 8080.
    Для Server.Api задаётся Agent__BaseUrl, чтобы команды выполнялись через агент.
    Перед запуском завершает уже работающие экземпляры и закрывает их окна консоли.
    Окна PowerShell с процессами запускаются свёрнутыми (панель задач).

.EXAMPLE
    .\scripts\start-server-and-agent.ps1

.EXAMPLE
    .\scripts\start-server-and-agent.ps1 -ServerPort 8080 -AgentPort 8090 -Prompt
#>
param(
    [int]$ServerPort = 8080,
    [int]$AgentPort = 8090,
    [string]$AgentApiKey = "CHANGE_ME_AGENT_KEY",
    [switch]$Prompt,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Path $PSScriptRoot -Parent
$hostAgentProjectPath = Join-Path $projectRoot "Host.Agent\Host.Agent.csproj"
$serverApiProjectPath = Join-Path $projectRoot "Server.Api\Server.Api.csproj"
$launcherPidFile = Join-Path $PSScriptRoot ".start-server-and-agent.pids"
$launcherMarkerHostAgent = "pc-monitor-launcher:host-agent"
$launcherMarkerServerApi = "pc-monitor-launcher:server-api"
$launcherMarkers = @($launcherMarkerHostAgent, $launcherMarkerServerApi)

function Resolve-Port {
    param(
        [string]$PromptText,
        [int]$DefaultPort
    )

    $portInput = Read-Host $PromptText
    if ([string]::IsNullOrWhiteSpace($portInput)) {
        return $DefaultPort
    }

    if ($portInput -notmatch '^\d+$') {
        Write-Host "Некорректный порт '$portInput'. Используется $DefaultPort." -ForegroundColor Yellow
        return $DefaultPort
    }

    $parsed = [int]$portInput
    if ($parsed -lt 1 -or $parsed -gt 65535) {
        Write-Host "Некорректный порт '$portInput'. Используется $DefaultPort." -ForegroundColor Yellow
        return $DefaultPort
    }

    return $parsed
}

function Stop-ProcessSafe {
    param([int]$ProcessId)

    if ($ProcessId -le 0) {
        return $false
    }

    $process = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue
    if (-not $process) {
        return $false
    }

    Write-Host "Остановка $($process.ProcessName) (PID $ProcessId)..." -ForegroundColor Yellow
    Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue
    return $true
}

function Stop-ProcessTree {
    param([int]$ProcessId)

    $children = @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
        Where-Object { $_.ParentProcessId -eq $ProcessId })

    foreach ($child in $children) {
        Stop-ProcessTree -ProcessId $child.ProcessId
    }

    [void](Stop-ProcessSafe -ProcessId $ProcessId)
}

function Stop-LauncherShells {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Markers
    )

    $stoppedAny = $false

    foreach ($shellName in @("powershell", "pwsh")) {
        $shellProcesses = @(Get-CimInstance Win32_Process -Filter "Name = '$shellName.exe'" -ErrorAction SilentlyContinue)
        foreach ($process in $shellProcesses) {
            $commandLine = $process.CommandLine
            if ([string]::IsNullOrWhiteSpace($commandLine)) {
                continue
            }

            $matchesMarker = $false
            foreach ($marker in $Markers) {
                if ($commandLine -like "*$marker*") {
                    $matchesMarker = $true
                    break
                }
            }

            if (-not $matchesMarker) {
                continue
            }

            if (Stop-ProcessSafe -ProcessId $process.ProcessId) {
                $stoppedAny = $true
            }
        }
    }

    return $stoppedAny
}

function Stop-PcMonitorProcesses {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ServerApiProjectPath,
        [Parameter(Mandatory = $true)]
        [string]$HostAgentProjectPath,
        [Parameter(Mandatory = $true)]
        [string]$LauncherPidFilePath,
        [Parameter(Mandatory = $true)]
        [string[]]$LauncherMarkers,
        [Parameter(Mandatory = $true)]
        [int[]]$Ports
    )

    $stoppedAny = $false

    if (Test-Path $LauncherPidFilePath) {
        $launcherPids = @(Get-Content -Path $LauncherPidFilePath -ErrorAction SilentlyContinue |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ -match '^\d+$' } |
            ForEach-Object { [int]$_ })

        foreach ($launcherPid in $launcherPids) {
            Stop-ProcessTree -ProcessId $launcherPid
            $stoppedAny = $true
        }

        Remove-Item -Path $LauncherPidFilePath -Force -ErrorAction SilentlyContinue
    }

    if (Stop-LauncherShells -Markers $LauncherMarkers) {
        $stoppedAny = $true
    }

    foreach ($processName in @("Server.Api", "Host.Agent")) {
        $processes = @(Get-Process -Name $processName -ErrorAction SilentlyContinue)
        foreach ($process in $processes) {
            if (Stop-ProcessSafe -ProcessId $process.Id) {
                $stoppedAny = $true
            }
        }
    }

    $projectMarkers = @(
        $ServerApiProjectPath,
        $HostAgentProjectPath,
        "Server.Api\Server.Api.csproj",
        "Host.Agent\Host.Agent.csproj"
    )

    $dotnetProcesses = @(Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue)
    foreach ($process in $dotnetProcesses) {
        $commandLine = $process.CommandLine
        if ([string]::IsNullOrWhiteSpace($commandLine)) {
            continue
        }

        $matchesProject = $false
        foreach ($marker in $projectMarkers) {
            if ($commandLine -like "*$marker*") {
                $matchesProject = $true
                break
            }
        }

        if (-not $matchesProject) {
            continue
        }

        if (Stop-ProcessSafe -ProcessId $process.ProcessId) {
            $stoppedAny = $true
        }
    }

    foreach ($port in $Ports) {
        $listeners = @(Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)
        foreach ($listener in $listeners) {
            $processId = $listener.OwningProcess
            if ($processId -le 0) {
                continue
            }

            if (Stop-ProcessSafe -ProcessId $processId) {
                $stoppedAny = $true
            }
        }
    }

    if (Stop-LauncherShells -Markers $LauncherMarkers) {
        $stoppedAny = $true
    }

    if ($stoppedAny) {
        Start-Sleep -Seconds 1
        Write-Host "Предыдущие экземпляры остановлены." -ForegroundColor DarkGray
    }
    else {
        Write-Host "Работающие экземпляры Host.Agent / Server.Api не найдены." -ForegroundColor DarkGray
    }
}

function Start-LauncherShell {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Command,
        [Parameter(Mandatory = $true)]
        [string]$Title
    )

    $arguments = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-Command", $Command
    )

    return Start-Process powershell `
        -ArgumentList $arguments `
        -PassThru `
        -WindowStyle Minimized
}

if ($Prompt) {
    $AgentPort = Resolve-Port -PromptText "Порт Host.Agent (Enter = $AgentPort)" -DefaultPort $AgentPort
    $ServerPort = Resolve-Port -PromptText "Порт Server.Api (Enter = $ServerPort)" -DefaultPort $ServerPort
}

if (-not (Test-Path $hostAgentProjectPath)) {
    Write-Host "Не найден проект: $hostAgentProjectPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $serverApiProjectPath)) {
    Write-Host "Не найден проект: $serverApiProjectPath" -ForegroundColor Red
    exit 1
}

Write-Host "Проверка запущенных экземпляров..." -ForegroundColor Cyan
Stop-PcMonitorProcesses `
    -ServerApiProjectPath $serverApiProjectPath `
    -HostAgentProjectPath $hostAgentProjectPath `
    -LauncherPidFilePath $launcherPidFile `
    -LauncherMarkers $launcherMarkers `
    -Ports @($ServerPort, $AgentPort)

if (-not $SkipBuild) {
    Write-Host "Сборка Host.Agent и Server.Api..." -ForegroundColor Cyan
    & dotnet build $hostAgentProjectPath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & dotnet build $serverApiProjectPath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$dotnetRunArgs = if ($SkipBuild) { "" } else { "--no-build" }

$agentBaseUrl = "http://127.0.0.1:$AgentPort/"

# Без -NoExit: окно закрывается, когда dotnet завершается (в т.ч. при остановке скриптом).
$agentCommand = @"
# $launcherMarkerHostAgent
`$host.UI.RawUI.WindowTitle = 'PcMonitor Host.Agent :$AgentPort'
Set-Location -LiteralPath '$projectRoot'
`$env:Agent__ApiKey = '$AgentApiKey'
Write-Host 'Host.Agent — http://0.0.0.0:$AgentPort' -ForegroundColor Green
dotnet run --project '$hostAgentProjectPath' $dotnetRunArgs -- --port $AgentPort
if (`$LASTEXITCODE -ne 0) { Read-Host 'Ошибка запуска. Нажмите Enter для закрытия' }
"@

$serverCommand = @"
# $launcherMarkerServerApi
`$host.UI.RawUI.WindowTitle = 'PcMonitor Server.Api :$ServerPort'
Set-Location -LiteralPath '$projectRoot'
`$env:Agent__BaseUrl = '$agentBaseUrl'
`$env:Agent__ApiKey = '$AgentApiKey'
Write-Host 'Server.Api — http://0.0.0.0:$ServerPort (агент: $agentBaseUrl)' -ForegroundColor Green
dotnet run --project '$serverApiProjectPath' $dotnetRunArgs -- --port $ServerPort
if (`$LASTEXITCODE -ne 0) { Read-Host 'Ошибка запуска. Нажмите Enter для закрытия' }
"@

Write-Host "Запуск Host.Agent на порту $AgentPort..." -ForegroundColor Green
$agentShell = Start-LauncherShell -Command $agentCommand -Title "PcMonitor Host.Agent"

Start-Sleep -Seconds 2

Write-Host "Запуск Server.Api на порту $ServerPort..." -ForegroundColor Green
$serverShell = Start-LauncherShell -Command $serverCommand -Title "PcMonitor Server.Api"

@($agentShell.Id, $serverShell.Id) | Set-Content -Path $launcherPidFile -Encoding ascii

Write-Host ""
Write-Host "Готово. Запущены два процесса (окна PowerShell свёрнуты в панели задач):" -ForegroundColor Cyan
Write-Host "  Host.Agent  → http://127.0.0.1:$AgentPort" -ForegroundColor White
Write-Host "  Server.Api  → http://127.0.0.1:$ServerPort" -ForegroundColor White
Write-Host "  Agent__BaseUrl = $agentBaseUrl" -ForegroundColor DarkGray
Write-Host ""
Write-Host "Токен: POST http://127.0.0.1:$ServerPort/api/auth/token?role=Operator" -ForegroundColor DarkGray
