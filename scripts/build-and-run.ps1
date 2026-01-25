param(
    [string]$Configuration = "Debug",
    [string]$ValheimPath = "C:\Program Files (x86)\Steam\steamapps\common\Valheim",
    [string]$SolutionPath = "$PSScriptRoot\..\EasySpawner.sln",
    [string]$MSBuildPath = "",
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

$pluginVersionSource = (Resolve-Path "$PSScriptRoot\..\EasySpawner\EasySpawnerPlugin.cs").ProviderPath
$pluginContent = Get-Content $pluginVersionSource -Raw
if ($pluginContent -match '\[BepInPlugin\(".*?",\s*".*?",\s*"([^"]+)"\)\]') {
    $version = $Matches[1]
} else {
    throw "Version not found in EasySpawnerPlugin.cs"
}

$dllPath = (Resolve-Path "$PSScriptRoot\..\EasySpawner\bin\$Configuration\EasySpawner.dll").ProviderPath
$pluginDir = Join-Path $ValheimPath "BepInEx\plugins\EasySpawner - $version"

Write-Host "Copying DLL to $pluginDir"
New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null
Copy-Item -Force $dllPath (Join-Path $pluginDir "EasySpawner.dll")

if (-not $NoLaunch) {
    $exePath = Join-Path $ValheimPath "valheim.exe"
    Write-Host "Launching Valheim: $exePath"
    Start-Process $exePath
} else {
    Write-Host "NoLaunch set; skipping game launch."
}

Write-Host "Done."
