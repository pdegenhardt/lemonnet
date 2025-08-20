@echo off
echo Building LEMON wrapper library for Windows...

REM Set the include path to the LEMON library
set LEMON_INCLUDE=/Ilemon-1.3.1

REM Check if Visual Studio is available
where cl >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Visual Studio compiler not found!
    echo Please run this script from a Visual Studio Developer Command Prompt
    exit /b 1
)

REM Compile the wrapper
cl /O2 /MD /LD /EHsc /std:c++14 ^
    %LEMON_INCLUDE% ^
    lemon_wrapper.cpp ^
    /Fe:lemon_wrapper.dll ^
    /DLEMON_WRAPPER_EXPORTS

if %ERRORLEVEL% EQU 0 (
    echo Build successful! Created lemon_wrapper.dll
    echo.
    echo To use in your C# application:
    echo 1. Make sure lemon_wrapper.dll is in the same directory as your executable
    echo 2. Or add its directory to PATH
) else (
    echo Build failed!
    exit /b 1
)