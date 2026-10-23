param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'bin'))
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (-not (Test-Path -LiteralPath $compiler)) { throw '.NET Framework C# 컴파일러를 찾을 수 없습니다.' }
$sourceFiles = (Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'src') -Filter '*.cs').FullName
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$outputFile = Join-Path $outputRoot '할일관리.exe'
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ "/out:$outputFile" "/win32manifest:$(Join-Path $PSScriptRoot 'app.manifest')" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Core.dll $sourceFiles
if ($LASTEXITCODE -ne 0) { throw '빌드 실패' }
if ($outputRoot -ne $PSScriptRoot) {Copy-Item -LiteralPath (Join-Path $PSScriptRoot '할일관리.exe.config') -Destination $outputRoot -Force}
Write-Output "빌드 완료: $outputFile"
