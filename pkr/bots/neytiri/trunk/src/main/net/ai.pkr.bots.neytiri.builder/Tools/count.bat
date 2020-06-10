@echo off
call set-env.bat

:: Process game logs

%ROOT%\CounterFellOmen2.Strategy.exe -count-games -logs:probe-logs\ 
		