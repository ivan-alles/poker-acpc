@echo off
del /Q /S build\* >nul
rmdir build\0 >nul
rmdir build\1 >nul

echo Move processed logs to back to probe-logs
move probe-logs-processed\* probe-logs >nul

