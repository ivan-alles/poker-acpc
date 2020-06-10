@echo off
:: Generate chance tree by MC.
:: Parameters: none
::
call set-env.bat
set GD=${bds.DataDir}ai.pkr.metastrategy.2/leduc-he.gamedef.xml


ctmcgen.1 -g:%GD% --add-ca-names- -o:%GEN_DIR% ca-leduc-full-game.xml  ca-leduc-full-game.xml 



