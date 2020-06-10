@echo off
:: Updates opponent data from new probe logs
:: If no opponent data is present, new files are created.
:: Processed logs are moved to probe-logs-processed

call set-env.bat

if not exist build mkdir build


set GAME_DEF=%AI_HOME%\data\ai.pkr.holdem.gd.fl.2.1.xml 
set BUCK=bucketizer.xml 

:: Process game logs

ai.pkr.bots.neytiri.builder -process-logs -logs:probe-logs -o:Fo2 -opp-act:build\opp-at.xml -game-def:%GAME_DEF% -bucketizer:%BUCK%

echo Move processed logs to probe-logs-processed
move probe-logs\* probe-logs-processed >nul


