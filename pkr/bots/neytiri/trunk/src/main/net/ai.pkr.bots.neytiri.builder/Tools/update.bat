@echo off
call set-env.bat

set GAME_DEF=%ROOT%\Config\CounterFellOmen2.GD.xml 
set BUCK=%ROOT%\Config\CounterFellOmen2.Bucketizer.xml 

:: Process game logs

%ROOT%\CounterFellOmen2.Strategy.exe -process-logs -logs:probe-logs\ -fo2-act:build\Fo2.At.xml -game-def:%GAME_DEF% -bucketizer:%BUCK%
