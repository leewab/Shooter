@echo off
chcp 65001 >nul
cd /d "%~dp0"
title Excel to C#/JSON Tool

echo ========================================
echo   Excel to C#/JSON Converter Tool
echo ========================================
echo.

REM 1. Check .NET environment
echo Step 1: Checking .NET environment...
where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: .NET 6.0 SDK not found
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo OK: .NET SDK found

REM 2. Restore NuGet packages
echo.
echo Step 2: Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: NuGet package restore failed
    pause
    exit /b 1
)
echo OK: Packages restored

REM 3. Build project
echo.
echo Step 3: Building project...
dotnet build --configuration Release
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo OK: Build successful

REM 4. Prepare directories
echo.
echo Step 4: Preparing directories...
if not exist "Excels" (
    echo Creating Excels directory...
    mkdir "Excels"
    echo Please put Excel files in Excels folder
)

if not exist "Output" mkdir "Output"
if not exist "Output\Models" mkdir "Output\Models"
if not exist "Output\Json" mkdir "Output\Json"

REM 5. Process Excel files
echo.
echo Step 5: Processing Excel files...
echo.

set EXE_PATH=bin\Release\net6.0\ExcelToJsonTool.exe
set /a SUCCESS_COUNT=0
set /a FAIL_COUNT=0

for %%f in ("Excels\*.xlsx") do (
    echo Processing: %%~nxf
    "%EXE_PATH%" "%%f" "Output\Models" "Output\Json" "GameConfig"
    
    if errorlevel 1 (
        echo [FAILED] %%~nxf
        set /a FAIL_COUNT+=1
    ) else (
        echo [SUCCESS] %%~nxf
        set /a SUCCESS_COUNT+=1
    )
    echo.
)

REM 6. Display results
echo ========================================
echo Processing complete!
echo Successful: %SUCCESS_COUNT% files
echo Failed: %FAIL_COUNT% files
echo ========================================
echo.

if %SUCCESS_COUNT% gtr 0 (
    echo Generated files:
    echo   Output\Models\  (C# class files)
    echo   Output\Json\    (JSON data files)
    echo.
    echo Next steps:
    echo   1. Add C# class files to your project
    echo   2. Install Newtonsoft.Json package
    echo   3. Load JSON files for configuration data
)

pause