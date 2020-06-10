@echo off
call setenv.bat

if not exist build mkdir build

:: Create seq records. Options: -pockets:"Tc 9d" 

ai.pkr.bots.neytiri.builder -seq-create -pockets:"Tc 9c"  -logs:probe-logs  -o:Fo2 -working-dir:build -game-def:%GAME_DEF% -buck-type:%BUCK_TYPE% -buck-params:File=bucketizer.xml

