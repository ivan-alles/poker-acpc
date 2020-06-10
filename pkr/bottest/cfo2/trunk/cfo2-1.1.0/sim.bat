@echo off 
call set-env.bat

start pkrserver.exe -p:9001 -l:logs ss-sim-rg.xml


