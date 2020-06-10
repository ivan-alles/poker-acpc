@echo off
call set-env.bat

ai.pkr.bots.neytiri.builder -print-neytiri-pf -neytiri:build\neytiri.xml >build\neytiri-pf.txt
