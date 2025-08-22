@echo off
REM This script builds the Linux native library using WSL

echo Building Linux native library via WSL...

REM Get the configuration from the first parameter, default to Debug
set CONFIGURATION=%1
if "%CONFIGURATION%"=="" set CONFIGURATION=Debug

REM Convert Windows path to WSL path and execute build
REM The path conversion happens inside WSL
wsl bash -c "cd /mnt/c/Dev/Claude/lemonnet && ./build.sh %CONFIGURATION%"

if %ERRORLEVEL% EQU 0 (
    echo Linux library build successful!
) else (
    echo Warning: Linux library build failed. This is non-fatal for Windows builds.
    REM We don't want to fail the entire build if WSL is not available
    exit /b 0
)