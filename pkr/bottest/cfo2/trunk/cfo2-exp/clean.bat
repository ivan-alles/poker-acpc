@echo off
del /Q build\*

echo Move processed logs to back to probe-logs
move probe-logs-processed\* probe-logs >nul

