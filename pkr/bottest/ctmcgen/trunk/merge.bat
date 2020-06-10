@echo off
:: Merges MC data.
:: Parameters: 
:: %1 merge target
:: %2-n merge sources

call setenv.bat


ctmcmerge  -o:%*




