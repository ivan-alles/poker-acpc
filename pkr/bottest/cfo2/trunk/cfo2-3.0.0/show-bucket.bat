@echo off
call setenv.bat

ai.pkr.bots.neytiri.builder -show-bucket -cards:%1  -game-def:%GAME_DEF% -buck-type:%BUCK_TYPE% -buck-params:File=bucketizer.xml

