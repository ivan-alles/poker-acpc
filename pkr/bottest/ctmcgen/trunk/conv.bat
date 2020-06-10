@echo off
:: Convert a ctmcgen file into a chance tree.
:: Parameters: 
:: %1 input file
:: %2 output file
call setenv.bat

ctmcconv.1 -o:%2 %1



