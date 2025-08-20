@echo off
echo Building LEMON wrapper library for Visual Studio (x64 and x86)...

REM Check if Visual Studio is available
where cl >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Visual Studio compiler not found!
    echo Please run this script from a Visual Studio Developer Command Prompt
    exit /b 1
)

REM Set the include path to the LEMON library
set LEMON_INCLUDE=/Ilemon-1.3.1

REM Create directories for platform-specific builds
if not exist x64 mkdir x64
if not exist x86 mkdir x86

echo.
echo Building x64 version...
echo ----------------------

REM Build x64 version
cl /O2 /MD /LD /EHsc /std:c++14 /DWIN64 ^
    %LEMON_INCLUDE% ^
    lemon_wrapper.cpp ^
    lemon-1.3.1\lemon\bits\windows.cc ^
    /Fe:x64\lemon_wrapper.dll ^
    /Fo:x64\ ^
    /DLEMON_WRAPPER_EXPORTS ^
    /DWIN32 /D_WIN32 /D_WINDOWS

if %ERRORLEVEL% NEQ 0 (
    echo x64 build failed!
    pause
    exit /b 1
)

echo x64 build successful!

echo.
echo Building x86 version...
echo ----------------------

REM Build x86 version
cl /O2 /MD /LD /EHsc /std:c++14 ^
    %LEMON_INCLUDE% ^
    lemon_wrapper.cpp ^
    lemon-1.3.1\lemon\bits\windows.cc ^
    /Fe:x86\lemon_wrapper.dll ^
    /Fo:x86\ ^
    /DLEMON_WRAPPER_EXPORTS ^
    /DWIN32 /D_WIN32 /D_WINDOWS

if %ERRORLEVEL% NEQ 0 (
    echo x86 build failed!
    pause
    exit /b 1
)

echo x86 build successful!

echo.
echo ======================================
echo Build complete!
echo.
echo Created:
echo   - x64\lemon_wrapper.dll (64-bit)
echo   - x86\lemon_wrapper.dll (32-bit)
echo.
echo To debug in Visual Studio:
echo   1. Open LemonNet.sln
echo   2. Set breakpoints in TestProgram.cs
echo   3. Press F5 to debug
echo ======================================