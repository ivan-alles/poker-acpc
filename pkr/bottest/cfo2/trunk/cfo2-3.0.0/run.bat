@echo off 
:: Runs a session with local bots.
:: Parameters:
:: %1 - session file

call setenv.bat

::call vsperfclrenv.cmd /sampleon

start pkrserver.exe -p:9001 -l:logs %1


