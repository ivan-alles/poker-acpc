@echo off
call setenv.bat

ai.pkr.bots.neytiri.builder -print-neytiri-pf -neytiri:build\neytiri.xml >build\neytiri-pf.txt
