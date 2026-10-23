$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path -LiteralPath $compiler)) { throw '.NET Framework C# 컴파일러를 찾을 수 없습니다.' }
$outputFile = Join-Path $PSScriptRoot '할일관리.exe'
$sourceFile = Join-Path $PSScriptRoot 'TodoApp.cs'
& $compiler /nologo /target:winexe "/out:$outputFile" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll $sourceFile
if ($LASTEXITCODE -ne 0) { throw '컴파일 실패' }
Write-Output "빌드 완료: $outputFile"
