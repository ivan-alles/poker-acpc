@echo off
:: Generate chance tree by MC.
:: Parameters: 
:: %1 chance abstraction 1
call setenv.bat


hecamcgen.exe --samples-count:0,50000,20000,20000  %1



