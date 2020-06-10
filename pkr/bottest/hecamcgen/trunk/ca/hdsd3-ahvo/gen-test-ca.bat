@echo off
:: Generate chance tree by MC.
:: Parameters: 
:: %1 chance abstraction 1
call setenv.bat


hecamcgen.exe --rng-seed:1 --samples-count:0,5000,5000,5000 test-ca.xml >gen-test-ca.log

run heca-clustertree-vis.exe test-ca.dat 

