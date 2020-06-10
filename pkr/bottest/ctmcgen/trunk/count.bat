@echo off
:: Counts leaves and samples in MC data.
:: Parameters: passes parameters to ctmcmerge
::

call setenv.bat


ctmcmerge -c %*




