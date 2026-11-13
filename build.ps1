$ErrorActionPreference = 'Stop'
$src = Join-Path $PSScriptRoot 'src'
$out = Join-Path $PSScriptRoot 'out'
New-Item -ItemType Directory -Path $out -Force | Out-Null
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
Push-Location -LiteralPath $src
try {
    & $csc /nologo /codepage:65001 /target:winexe /platform:anycpu /win32manifest:app.manifest /win32icon:app.ico /resource:app-icon.png,app-mark /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/out:$out\视频号下载助手.exe" Launcher.cs Artwork.cs ServiceTelemetry.cs FrameClock.cs
    if($LASTEXITCODE -ne 0){throw 'Build failed'}
} finally { Pop-Location }
