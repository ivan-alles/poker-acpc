@echo off
:: Converts a cluster tree to a GV file.
:: Parameters: 
:: %1 cluster tree file
call setenv.bat

run heca-clustertree-vis %1