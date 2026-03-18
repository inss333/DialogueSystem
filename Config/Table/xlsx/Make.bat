@echo off
setlocal EnableExtensions

set "SCRIPT_DIR=%~dp0"
set "ROOT_DIR=%SCRIPT_DIR%"
set "PROJECT_ROOT=%ROOT_DIR%..\..\.."
set "TABTOY_SRC=%ROOT_DIR%..\..\tabtoy"
set "TABTOY_BIN=%ROOT_DIR%tabtoy.exe"
set "TABTOY_PREBUILT1=%TABTOY_SRC%\bin\windows\tabtoy.exe"
set "TABTOY_PREBUILT2=%ROOT_DIR%..\tabtoy.exe"
set "CSHARP_OUT=%PROJECT_ROOT%\Assets\Scripts\Core\Table\Generated\table_gen.cs"
set "BINARY_DIR_OUT=%PROJECT_ROOT%\Assets\Resources\LoadableAssets\Table"

cd /d "%ROOT_DIR%"

if "%GOPROXY%"=="" set "GOPROXY=https://goproxy.cn,direct"

echo [INFO] ensure output dir...
if not exist "%PROJECT_ROOT%\Assets\Scripts\Core\Table\Generated" mkdir "%PROJECT_ROOT%\Assets\Scripts\Core\Table\Generated"
if not exist "%BINARY_DIR_OUT%" mkdir "%BINARY_DIR_OUT%"

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

echo [INFO] export csharp and binary...
"%TABTOY_BIN%" -mode=v3 ^
  -index="%ROOT_DIR%Index.xlsx" ^
  -csharp_out="%CSHARP_OUT%" ^
  -binary_dir="%BINARY_DIR_OUT%" ^
  -package=D1
if errorlevel 1 goto :fail

echo [INFO] rename binary files...
powershell -NoProfile -Command "$dir='%BINARY_DIR_OUT%'; Get-ChildItem -Path $dir -Filter '*.bin' -File | ForEach-Object { $newPath = [System.IO.Path]::ChangeExtension($_.FullName, '.bytes'); if (Test-Path $newPath) { Remove-Item $newPath -Force }; Move-Item $_.FullName $newPath }"
if errorlevel 1 goto :fail

echo [INFO] build tablelist.json...
powershell -NoProfile -Command "$dir='%BINARY_DIR_OUT%'; $tables = Get-ChildItem -Path $dir -Filter '*.bytes' -File | Sort-Object Name | ForEach-Object { $_.Name }; $tables | ConvertTo-Json -Depth 2 | Set-Content -Path (Join-Path $dir 'tablelist.json') -Encoding UTF8"
if errorlevel 1 goto :fail

del /f /q "%TABTOY_BIN%" >nul 2>nul

echo [INFO] Make succeeded.
echo [INFO] output: %CSHARP_OUT%
echo [INFO] output: %BINARY_DIR_OUT%
pause
exit /b 0

:fail
echo [ERROR] Make failed, code=%ERRORLEVEL%
pause
exit /b 1
