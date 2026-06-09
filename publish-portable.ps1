$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$dotnet = Join-Path $root '.dotnet\dotnet.exe'
if (-not (Test-Path $dotnet)) {
  $siblingDotnet = Join-Path (Split-Path -Parent $root) 'dotnet\dotnet.exe'
  if (Test-Path $siblingDotnet) {
    $dotnet = $siblingDotnet
  } else {
    $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($cmd) {
      $dotnet = $cmd.Source
    } else {
      $installDir = Join-Path $root '.dotnet'
      $script = Join-Path $root 'dotnet-install.ps1'
      Write-Host 'SDK .NET nao encontrado. Baixando SDK .NET 8 localmente...'
      Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $script
      powershell -ExecutionPolicy Bypass -File $script -Channel 8.0 -InstallDir $installDir
      $dotnet = Join-Path $installDir 'dotnet.exe'
    }
  }
}

$runtime = 'win-x86'
$publishDir = Join-Path $root "publish\$runtime-portable"
if (Test-Path $publishDir) {
  Remove-Item -LiteralPath $publishDir -Recurse -Force
}

& $dotnet publish (Join-Path $root 'AssinadorPmenos.csproj') `
  -c Release `
  -r $runtime `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  -o $publishDir

if ($LASTEXITCODE -ne 0) {
  exit $LASTEXITCODE
}

Write-Host "Publicado: $(Join-Path $publishDir 'assinador_pmenos.exe')"
