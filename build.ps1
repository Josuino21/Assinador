$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
& powershell -ExecutionPolicy Bypass -File (Join-Path $root 'publish-portable.ps1')
