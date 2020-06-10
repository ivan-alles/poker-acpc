@echo off 
:: Runs a session.
:: Parameters:
:: %1 - session file (relative path)

call setenv.bat

::call vsperfclrenv.cmd /sampleon

:: We have to change directory to bin directory, otherwise there are problems with
:: creating types by assembly name.

pushd %BOTTEST_BIN%

start pkrserver.exe -p:9001 -l:%BOTTEST_ROOT%logs --on-session-suite-end:%BOTTEST_ROOT%\on-session-suite-end.bat %BOTTEST_ROOT%\%1 

popd


