@echo off
:: Updates opponent data from new probe logs
:: If no opponent data is present, new files are created.
:: Processed logs are moved to probe-logs-processed

call setenv.bat

if not exist build mkdir build

:: Process game logs

ai.pkr.bots.neytiri.builder -process-logs -logs:probe-logs -o:Fo2 -opp-act:build\opp-at.xml -game-def:%GAME_DEF% -buck-type:%BUCK_TYPE% -buck-params:File=bucketizer.xml

echo Move processed logs to probe-logs-processed
move probe-logs\* probe-logs-processed >nul


