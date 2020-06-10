@echo off
call set-env.bat

set GAME_DEF=%ROOT%\Config\CounterFellOmen2.GD.xml 
set BUCK=%ROOT%\Config\CounterFellOmen2.Bucketizer.xml 

%ROOT%\CounterFellOmen2.Strategy.exe -monte-carlo -mc-count:3000 -neytiri:build\Neytiri.Str.xml -game-def:%GAME_DEF% -bucketizer:%BUCK%
