@echo off
:: Create initial configuration for the solver.
:: Parameters: none
::
call setenv.bat

::set GD=${bds.DataDir}ai.pkr.metastrategy.2/leduc-he.gamedef.xml
set GD=${bds.DataDir}ai.pkr.holdem.gamedef.2/holdem-gd-fl-2.xml 

pkrtree -g:%GD%  --tree:action   -o:at.dat

