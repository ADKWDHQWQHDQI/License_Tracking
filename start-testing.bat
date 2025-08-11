@echo off
echo Starting License Tracking Application Test...
cd "c:\Users\sandeepk\Downloads\Project\License_Tracking"

echo Building application...
dotnet build License_Tracking.csproj --verbosity quiet
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    exit /b 1
)

echo Starting application for 10 seconds...
timeout /t 2 /nobreak >nul
start /B dotnet run > startup_log.txt 2>&1

echo Waiting 10 seconds for startup...
ping 127.0.0.1 -n 11 > nul

echo Checking for database errors...
findstr /C:"Invalid column name" startup_log.txt
if %ERRORLEVEL% equ 0 (
    echo ERROR: Database column issues found!
) else (
    echo SUCCESS: No database column errors detected!
)

echo Stopping application...
taskkill /f /im dotnet.exe > nul 2>&1

echo Test completed.
