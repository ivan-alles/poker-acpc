@echo off
call setenv.bat

if exist build\neytiri.xml goto update

echo Creating neytiri file from opponent's data.
copy /Y build\opp-at.xml build\neytiri.xml

:update

SET R=%1
FOR /L %%I IN (0,1,%R%) DO ai.pkr.bots.neytiri.builder -monte-carlo -mc-count:1,100,5 -neytiri:build\neytiri.xml -pockets:Tc9d -position:0

:: -pockets:Tc9d
:: -pockets:AcAd

