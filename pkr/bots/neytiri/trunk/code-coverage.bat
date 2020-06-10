@echo off
REM Does code-coverage analysis
REM Run from the project root without parameters.

set OUT_DIR=target\test-reports
if not exist %OUT_DIR%  mkdir %OUT_DIR%
start "vsperfmon" vsperfmon.exe /coverage /output:%OUT_DIR%\code-coverage.coverage
call mvn test -Pcode-coverage
vsperfcmd /shutdown

