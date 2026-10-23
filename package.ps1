param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'dist'), [string]$RuntimeDirectory = (Join-Path $PSScriptRoot 'bin'))
$ErrorActionPreference = 'Stop'
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$stage = Join-Path ([IO.Path]::GetTempPath()) ('WorkTodoPackage-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
$runtimeFiles = @('할일관리.exe','할일관리.exe.config','사용안내.md','VERSION.txt')
foreach ($name in $runtimeFiles) { $sourceRoot=if($name -eq '할일관리.exe' -or $name -eq '할일관리.exe.config') {$RuntimeDirectory} else {$PSScriptRoot}; Copy-Item -LiteralPath (Join-Path $sourceRoot $name) -Destination $stage}
Get-ChildItem -LiteralPath $stage -File | ForEach-Object { $hash = Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256; $hash.Hash + '  ' + $_.Name } | Set-Content -LiteralPath (Join-Path $stage 'SHA256SUMS.txt') -Encoding utf8
Compress-Archive -LiteralPath (Get-ChildItem -LiteralPath $stage -File).FullName -DestinationPath (Join-Path $outputRoot 'work-todo-windows.zip') -Force
$sourceStage = Join-Path ([IO.Path]::GetTempPath()) ('WorkTodoSource-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $sourceStage | Out-Null
$sourceFiles = @('AGENTS.md','STATUS.md','CHANGELOG.md','README.md','.gitignore','build.ps1','package.ps1','app.manifest','할일관리.exe.config','사용안내.md','VERSION.txt','TEST_RESULTS.md')
foreach ($name in $sourceFiles) {Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $sourceStage}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'src') -Destination $sourceStage -Recurse
Compress-Archive -Path (Join-Path $sourceStage '*') -DestinationPath (Join-Path $outputRoot 'work-todo-source.zip') -Force
Get-ChildItem -LiteralPath $outputRoot -Filter 'work-todo-*.zip' | Select-Object Name,Length


