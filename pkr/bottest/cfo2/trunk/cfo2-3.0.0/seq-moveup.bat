@echo off
call setenv.bat

if not exist build\neytiri.xml copy /Y build\opp-at.xml build\neytiri.xml

ai.pkr.bots.neytiri.builder -seq-moveup -working-dir:build -neytiri:build\neytiri.xml



