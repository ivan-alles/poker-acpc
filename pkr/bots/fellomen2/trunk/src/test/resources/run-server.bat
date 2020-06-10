@echo off
:: Runs a session suite.
:: Parameters:
:: %1 session suite cfg xml file.
::

call set-env.bat

pkrserver %1


                        
