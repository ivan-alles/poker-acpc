:@echo off
rem Creates and installs empty lookup tables to resolve cyclic dependencies.


set TARGET_DIR=target\dist\data
set TARGET0=%TARGET_DIR%\hs-lut.preflop.dat
set TARGET1=%TARGET_DIR%\hs-lut.flop.dat
set TARGET2=%TARGET_DIR%\hs-lut.turn.dat

if not exist %TARGET_DIR% mkdir %TARGET_DIR%
if exist %TARGET0% goto ALREADY_EXIST
if exist %TARGET1% goto ALREADY_EXIST
if exist %TARGET2% goto ALREADY_EXIST

echo dummy >%TARGET0%
echo dummy >%TARGET1%
echo dummy >%TARGET2%

mvn install 

goto END

:ALREADY_EXIST
echo One of the target files already exists, no installation is done to preserve possibly good artifacts.
echo Clean manually to create a dummy file.


:END
