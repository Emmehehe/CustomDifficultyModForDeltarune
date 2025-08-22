@echo off
setlocal

where pwsh >nul 2>&1
if %errorlevel% equ 0 (
  set "PS=pwsh"
) else (
  set "PS=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"
)

"%PS%" -NoProfile -ExecutionPolicy Bypass -File "%~dp0install-windows.ps1" %*
exit /b %errorlevel%
