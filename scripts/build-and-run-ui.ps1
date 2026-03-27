param(
    [switch]$Release
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Path $PSScriptRoot -Parent
$desktopProjectPath = Join-Path $projectRoot "Client.Avalonia\Client.Avalonia.Desktop\Client.Avalonia.Desktop.csproj"
$androidProjectPath = Join-Path $projectRoot "Client.Avalonia\Client.Avalonia.Android\Client.Avalonia.Android.csproj"

$configuration = if ($Release) { "Release" } else { "Debug" }
$androidTargetFramework = "net9.0-android"
$androidPackageId = "com.CompanyName.Client.Avalonia"

function Get-AdbPath {
    $sdk = $env:ANDROID_HOME
    if (-not $sdk) { $sdk = $env:ANDROID_SDK_ROOT }
    if (-not $sdk) { $sdk = "$env:LOCALAPPDATA\Android\Sdk" }

    $adbPath = Join-Path $sdk "platform-tools\adb.exe"
    if (Test-Path $adbPath) { return $adbPath }

    if (Get-Command adb -ErrorAction SilentlyContinue) { return "adb" }

    Write-Host "Ошибка: adb не найден. Установите Android SDK Platform-Tools или добавьте adb в PATH." -ForegroundColor Red
    exit 1
}

function Invoke-AdbShellSafe {
    param(
        [Parameter(Mandatory = $true)]
        [string]$adb,
        [Parameter(Mandatory = $true)]
        [string]$serial,
        [Parameter(Mandatory = $true)]
        [string]$command
    )

    try {
        return & $adb -s $serial shell $command 2>$null
    }
    catch {
        return $null
    }
}

function Get-AndroidDevicesWithInfo {
    param(
        [Parameter(Mandatory = $true)]
        [string]$adb
    )

    $deviceLines = & $adb devices -l 2>$null | Where-Object { $_ -match "^\S+\s+device\b" }
    $result = @()

    foreach ($line in $deviceLines) {
        $serial = ($line -split "\s+")[0]
        if ([string]::IsNullOrWhiteSpace($serial)) { continue }

        $mfr = Invoke-AdbShellSafe -adb $adb -serial $serial -command "getprop ro.product.manufacturer"
        $manufacturer = if ($mfr) { $mfr.Trim() } else { $null }
        $mdl = Invoke-AdbShellSafe -adb $adb -serial $serial -command "getprop ro.product.model"
        $model = if ($mdl) { $mdl.Trim() } else { $null }
        $name = if ($manufacturer -and $model) { "$manufacturer $model" } elseif ($model) { $model } else { $serial }

        $ip = $null
        $ipOut = Invoke-AdbShellSafe -adb $adb -serial $serial -command "ip -4 addr show wlan0 2>/dev/null"
        if ($ipOut -match "inet\s+(\d+\.\d+\.\d+\.\d+)") { $ip = $Matches[1] }
        if (-not $ip) {
            $ipOut = Invoke-AdbShellSafe -adb $adb -serial $serial -command "ip -4 addr"
            foreach ($match in ([regex]::Matches($ipOut, "inet\s+(\d+\.\d+\.\d+\.\d+)"))) {
                $candidate = $match.Groups[1].Value
                if ($candidate -ne "127.0.0.1") { $ip = $candidate; break }
            }
        }
        $ipDisplay = if ($ip) { $ip } else { "—" }

        $result += [PSCustomObject]@{
            Serial = $serial
            Name   = $name
            IP     = $ipDisplay
        }
    }

    return $result
}

function Select-AndroidDevice {
    param(
        [Parameter(Mandatory = $true)]
        [string]$adb
    )

    Write-Host "`nСпособ запуска Android:" -ForegroundColor Cyan
    Write-Host "  1) USB      — устройства по кабелю"
    Write-Host "  2) Wi-Fi    — устройства по adb connect"
    Write-Host "  3) Эмулятор — запущенные эмуляторы"

    $mode = $null
    do {
        $inputMode = Read-Host "Выберите 1, 2 или 3"
        if ($inputMode -eq "1") { $mode = "USB"; break }
        if ($inputMode -eq "2") { $mode = "WiFi"; break }
        if ($inputMode -eq "3") { $mode = "Emulator"; break }
        Write-Host "Введите 1, 2 или 3." -ForegroundColor Yellow
    } while ($true)

    $allDevices = Get-AndroidDevicesWithInfo -adb $adb
    if ($allDevices.Count -eq 0) {
        Write-Host "Нет доступных Android-устройств." -ForegroundColor Red
        exit 1
    }

    switch ($mode) {
        "USB"      { $devices = @($allDevices | Where-Object { $_.Serial -notmatch ":" -and $_.Serial -notmatch "^emulator-" }) }
        "WiFi"     { $devices = @($allDevices | Where-Object { $_.Serial -match ":" }) }
        "Emulator" { $devices = @($allDevices | Where-Object { $_.Serial -match "^emulator-" }) }
    }

    if ($devices.Count -eq 0) {
        Write-Host "Устройств для выбранного режима не найдено." -ForegroundColor Red
        exit 1
    }

    Write-Host "`nПодключённые Android-устройства:" -ForegroundColor Cyan
    Write-Host ("-" * 60)
    for ($i = 0; $i -lt $devices.Count; $i++) {
        $device = $devices[$i]
        Write-Host "  $($i + 1) Устройство: " -NoNewline
        Write-Host $device.Name -ForegroundColor Green
        Write-Host "      ID:       $($device.Serial)"
        Write-Host "      IP:       $($device.IP)"
        Write-Host ("-" * 60)
    }

    do {
        $userInput = Read-Host "Введите номер устройства (1-$($devices.Count))"
        $selected = 0
        if ([int]::TryParse($userInput, [ref]$selected) -and $selected -ge 1 -and $selected -le $devices.Count) {
            $deviceId = $devices[$selected - 1].Serial
            Write-Host "Выбрано: $($devices[$selected - 1].Name)" -ForegroundColor Gray
            return $deviceId
        }
        Write-Host "Введите число от 1 до $($devices.Count)." -ForegroundColor Yellow
    } while ($true)
}

Write-Host "Целевая платформа UI:" -ForegroundColor Cyan
Write-Host "  1) Desktop"
Write-Host "  2) Android"

$platform = $null
do {
    $inputPlatform = Read-Host "Выберите 1 или 2"
    if ($inputPlatform -eq "1") { $platform = "Desktop"; break }
    if ($inputPlatform -eq "2") { $platform = "Android"; break }
    Write-Host "Введите 1 или 2." -ForegroundColor Yellow
} while ($true)

if ($platform -eq "Desktop") {
    Write-Host "`nСборка UI под Desktop ($configuration)..." -ForegroundColor Cyan
    & dotnet build $desktopProjectPath -c $configuration
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host "Запуск Desktop-приложения..." -ForegroundColor Cyan
    & dotnet run --project $desktopProjectPath -c $configuration
    exit $LASTEXITCODE
}

$adb = Get-AdbPath
$deviceId = Select-AndroidDevice -adb $adb

Write-Host "`nСборка UI под Android ($configuration)..." -ForegroundColor Cyan
& dotnet build $androidProjectPath -c $configuration -f $androidTargetFramework -p:AndroidBuildApplicationPackage=True
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$apkDirectory = Join-Path $projectRoot ("Client.Avalonia\Client.Avalonia.Android\bin\{0}\{1}" -f $configuration, $androidTargetFramework)
$apk = Get-ChildItem -Path $apkDirectory -Recurse -Filter "*.apk" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $apk) {
    Write-Host "Ошибка: APK не найден после сборки." -ForegroundColor Red
    exit 1
}

Write-Host "Удаление старой версии приложения (если была)..." -ForegroundColor Cyan
& $adb -s $deviceId uninstall $androidPackageId 2>$null

Write-Host "Установка APK..." -ForegroundColor Cyan
& $adb -s $deviceId install $apk.FullName
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Запуск приложения..." -ForegroundColor Cyan
& $adb -s $deviceId shell monkey -p $androidPackageId -c android.intent.category.LAUNCHER 1
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "`nГотово." -ForegroundColor Green
