@echo off
echo Building Binder Tool...
echo.

echo Restoring dependencies...
dotnet restore
if %errorlevel% neq 0 (
    echo Error: Failed to restore dependencies
    pause
    exit /b 1
)

echo.
echo Building single-file executable...
dotnet publish --configuration Release --output ./publish --self-contained true --runtime win-x64
if %errorlevel% neq 0 (
    echo Error: Build failed
    pause
    exit /b 1
)

echo.
echo Copying executable to current directory...
copy publish\Binder.exe . >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: Failed to copy executable
    pause
    exit /b 1
)

echo.
echo Cleaning up temporary files...
rmdir /s /q publish >nul 2>&1

echo.
echo Build completed successfully!
echo.
echo Single-file executable created: Binder.exe
echo.
echo To run the application:
echo 1. Run: Binder.exe
echo 2. Or use: dotnet run
echo.
pause 