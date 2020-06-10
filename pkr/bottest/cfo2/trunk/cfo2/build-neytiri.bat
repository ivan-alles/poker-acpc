@echo off
call set-env.bat

echo Creating neytiri file from opponent's data.
copy /Y build\opp-at.xml build\neytiri.xml

ai.pkr.bots.neytiri.builder -monte-carlo -mc-count:100000 -neytiri:build\neytiri.xml 

:: -pockets:AcAd

