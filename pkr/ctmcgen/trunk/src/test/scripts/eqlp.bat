@echo off
:: Solve EQ by LP
:: Parameters: 
:: %1 Chance tree file

call set-env.bat
set GD=${bds.DataDir}ai.pkr.metastrategy.2/leduc-he.gamedef.xml


eqlp.exe -g:%GD%  --chance-tree:%1 -o:%1 




