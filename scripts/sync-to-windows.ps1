param(
  [switch]$Build,
  [string]$Run,
  [string]$Dir = "C:\dev\pixel-factory"
)

$ErrorActionPreference = "Stop"

# Find WSL source path
$wslPath = wsl wslpath -a (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
if (-not $wslPath) {
  $wslPath = "\\wsl$\Ubuntu\home\$env:USERNAME\code\pixel-factory"
}

Write-Host "Syncing from WSL to $Dir ..."
if (-not (Test-Path $Dir)) { New-Item -ItemType Directory -Path $Dir -Force | Out-Null }

robocopy $wslPath $Dir /MIR /XD bin obj .git /NFL /NDL /NJH /NJS /NS /NC
if ($LASTEXITCODE -ge 8) { throw "robocopy failed with code $LASTEXITCODE" }

Write-Host "Sync complete."

if ($Build) {
  Write-Host "Building..."
  dotnet build "$Dir\PixelFactory.slnx"
}

if ($Run) {
  Write-Host "Running $Run..."
  dotnet run --project "$Dir\src\$Run\$Run.csproj"
}
