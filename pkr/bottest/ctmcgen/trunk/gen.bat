@echo off
:: Generate chance tree by MC.
:: Parameters: 
:: %1 chance abstraction 1
:: %2 chance abstraction 2
call setenv.bat


start ctmcgen.exe -g:%GD% --samples-count:200000000  -o:%CTMCGEN_GEN_DIR% %1 %2



