@echo off
setlocal EnableExtensions

set "SCRIPT_DIR=%~dp0"
set "ROOT_DIR=%SCRIPT_DIR%"
set "PROJECT_ROOT=%ROOT_DIR%..\.."
set "TABTOY_SRC=%ROOT_DIR%..\..\tabtoy"
set "TABTOY_BIN=%ROOT_DIR%tabtoy.exe"
set "TABTOY_PREBUILT1=%TABTOY_SRC%\bin\windows\tabtoy.exe"
set "TABTOY_PREBUILT2=%ROOT_DIR%..\tabtoy.exe"
set "CSHARP_OUT=%PROJECT_ROOT%\Assets\Scripts\Generated\table_gen.cs"
set "JSON_DIR_OUT=%PROJECT_ROOT%\Assets\Resources\LoadableAssets\Table"

cd /d "%ROOT_DIR%"

if "%GOPROXY%"=="" set "GOPROXY=https://goproxy.cn,direct"

echo [INFO] ensure output dir...
if not exist "%PROJECT_ROOT%\Assets\Scripts\Generated" mkdir "%PROJECT_ROOT%\Assets\Scripts\Generated"
if not exist "%JSON_DIR_OUT%" mkdir "%JSON_DIR_OUT%"

if exist "%TABTOY_PREBUILT2%" (
  echo [INFO] use prebuilt tabtoy: %TABTOY_PREBUILT2%
  copy /y "%TABTOY_PREBUILT2%" "%TABTOY_BIN%" >nul
  if errorlevel 1 goto :fail
) else if exist "%TABTOY_PREBUILT1%" (
  echo [INFO] use prebuilt tabtoy: %TABTOY_PREBUILT1%
  copy /y "%TABTOY_PREBUILT1%" "%TABTOY_BIN%" >nul
  if errorlevel 1 goto :fail
) else (
  where go >nul 2>nul
  if errorlevel 1 (
    echo [ERROR] go not found and prebuilt exe not found.
    echo [ERROR] place tabtoy.exe in either:
    echo         %TABTOY_PREBUILT2%
    echo         %TABTOY_PREBUILT1%
    goto :fail
  )

  if not exist "%TABTOY_SRC%" (
    echo [ERROR] tabtoy source not found: %TABTOY_SRC%
    goto :fail
  )

  for /f "delims=" %%i in ('go version 2^>nul') do set "GOVER=%%i"
  echo [INFO] %GOVER%

  echo [INFO] build tabtoy...
  go build -v -o "%TABTOY_BIN%" "%TABTOY_SRC%"
  if errorlevel 1 goto :fail
)

echo [INFO] export csharp and json...
"%TABTOY_BIN%" -mode=v3 ^
  -index="%ROOT_DIR%Index.xlsx" ^
  -csharp_out="%CSHARP_OUT%" ^
  -json_dir="%JSON_DIR_OUT%" ^
  -package=D1
if errorlevel 1 goto :fail

echo [INFO] build tablelist.json...
powershell -NoProfile -Command "$dir='%JSON_DIR_OUT%'; $tables = Get-ChildItem -Path $dir -Filter '*.json' -File | Where-Object { $_.Name -ne 'tablelist.json' } | Sort-Object Name | ForEach-Object { $_.Name }; $obj = @{ Tables = @($tables) }; $obj | ConvertTo-Json -Depth 3 | Set-Content -Path (Join-Path $dir 'tablelist.json') -Encoding UTF8"
if errorlevel 1 goto :fail

echo [INFO] strip tabtoy runtime dependency...
powershell -NoProfile -Command "$p='%CSHARP_OUT%'; if(Test-Path $p){$c=Get-Content -Raw -Path $p; $c=$c -replace ' : tabtoy\.ITableSerializable',''; $lines=$c -split \"`r?`n\"; $out=foreach($line in $lines){ if($line -match '^\s*public partial class '){ '[System.Serializable]'; $line } else { $line } }; $c=[string]::Join(\"`r`n\",$out); Set-Content -Path $p -Value $c -Encoding UTF8}"
if errorlevel 1 goto :fail

del /f /q "%TABTOY_BIN%" >nul 2>nul

echo [INFO] Make succeeded.
echo [INFO] output: %CSHARP_OUT%
echo [INFO] output: %JSON_DIR_OUT%
pause
exit /b 0

:fail
echo [ERROR] Make failed, code=%ERRORLEVEL%
pause
exit /b 1
