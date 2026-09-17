# Fix-WpfBuild.ps1
# Run this script from the repository root after extracting/cloning it.
$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot 'src'
if (-not (Test-Path $src)) { throw "src folder not found. Run this script from the project root." }

# WPF StackPanel, Grid, ScrollViewer, ContentControl and GroupBox do not expose
# a Padding dependency property in the WPF version used by this project.
Get-ChildItem -Path $src -Filter '*.xaml' -Recurse | ForEach-Object {
    $text = Get-Content -LiteralPath $_.FullName -Raw
    $text = [regex]::Replace($text, '\s+Padding="[^"]*"', '')
    Set-Content -LiteralPath $_.FullName -Value $text -Encoding UTF8
}

$project = Join-Path $src 'FortnitePerformanceOptimizer.csproj'
$text = Get-Content -LiteralPath $project -Raw
$text = [regex]::Replace($text, '<TrimMode>.*?</TrimMode>\s*', '')
$text = [regex]::Replace($text, '<PublishTrimmed>true</PublishTrimmed>', '<PublishTrimmed>false</PublishTrimmed>')
$text = [regex]::Replace($text, '<PublishReadyToRun>true</PublishReadyToRun>', '<PublishReadyToRun>false</PublishReadyToRun>')
Set-Content -LiteralPath $project -Value $text -Encoding UTF8

Push-Location $src
try {
    dotnet clean
    dotnet restore
    dotnet build -c Release
}
finally { Pop-Location }

Write-Host "Build fix complete. If build succeeded, start with:" -ForegroundColor Green
Write-Host "dotnet run -c Release --no-build" -ForegroundColor Green
