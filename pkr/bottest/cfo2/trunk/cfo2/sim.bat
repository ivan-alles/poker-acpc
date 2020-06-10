@echo off 
call set-env.bat

start pkrserver.exe -p:9002 -l:logs ss-sim-rg.xml


