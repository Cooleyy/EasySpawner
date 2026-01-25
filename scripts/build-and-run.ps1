param(
    [string]$Configuration = "Debug",
    [string]$SolutionPath = "$PSScriptRoot\..\EasySpawner.sln",
    [string]$MSBuildPath = "",
    [string]$EnvironmentPropsPath = "$PSScriptRoot\..\Environment.props",
    [switch]$NoLaunch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($MSBuildPath)) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $MSBuildPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
    }
}

if ([string]::IsNullOrWhiteSpace($MSBuildPath)) {
    $MSBuildPath = "msbuild"
}

$solutionPathResolved = (Resolve-Path $SolutionPath).ProviderPath

Write-Host "Building $solutionPathResolved ($Configuration) with $MSBuildPath..."
& $MSBuildPath $solutionPathResolved -t:Restore -p:Configuration=$Configuration
& $MSBuildPath $solutionPathResolved -p:Configuration=$Configuration

$envPropsResolved = (Resolve-Path $EnvironmentPropsPath).ProviderPath
[xml]$envProps = Get-Content $envPropsResolved -Raw
$valheimPath = $envProps.Project.PropertyGroup.VALHEIM_INSTALL
if ([string]::IsNullOrWhiteSpace($valheimPath)) {
    throw "VALHEIM_INSTALL is not set in Environment.props"
}

$pluginVersionSource = (Resolve-Path "$PSScriptRoot\..\EasySpawner\EasySpawnerPlugin.cs").ProviderPath
$pluginContent = Get-Content $pluginVersionSource -Raw
if ($pluginContent -match '\[BepInPlugin\(".*?",\s*".*?",\s*"([^"]+)"\)\]') {
    $version = $Matches[1]
} else {
    throw "Version not found in EasySpawnerPlugin.cs"
}

$dllPath = (Resolve-Path "$PSScriptRoot\..\EasySpawner\bin\$Configuration\EasySpawner.dll").ProviderPath
$pluginDir = Join-Path $valheimPath "BepInEx\plugins\EasySpawner - $version"

Write-Host "Closing Valheim if running..."
Get-Process -Name "valheim" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
try {
    $valheimProc = Get-Process -Name "valheim" -ErrorAction Stop
    $valheimProc.WaitForExit(10000) | Out-Null
} catch {
    # Not running
}

Write-Host "Copying DLL to $pluginDir"
New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null
$destDll = Join-Path $pluginDir "EasySpawner.dll"
$maxAttempts = 5
for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
    try {
        Copy-Item -Force $dllPath $destDll
        break
    } catch {
        if ($attempt -eq $maxAttempts) {
            throw
        }
        Write-Host "Copy failed (attempt $attempt). Retrying in 1s..."
        Start-Sleep -Seconds 1
    }
}

if (-not $NoLaunch) {
    $exePath = Join-Path $valheimPath "valheim.exe"
    Write-Host "Launching Valheim: $exePath"
    Start-Process $exePath
} else {
    Write-Host "NoLaunch set; skipping game launch."
}

Write-Host "Done."
