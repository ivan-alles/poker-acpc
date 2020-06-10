@echo off
:: Merges MC data.
:: Parameters: none
::
call set-env.bat


ctmcmerge.1  -o:merged.dat  %GEN_DIR%




