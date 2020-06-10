@echo off
:: Starts pkrserver and FellOmen2 
:: Parameters:
:: %1 session suite configuration file
:: %2 TCP Port

call setenv.bat


:: We have to change directory to bin directory, otherwise there are problems with
:: creating types by assembly name.

pushd %BOTTEST_BIN%

start /low pkrserver.exe -p:%2 -l:%BOTTEST_ROOT%logs --on-session-suite-end:%BOTTEST_ROOT%\on-session-suite-end.bat %BOTTEST_ROOT%\%1 

popd


:: A poor man wait.
ping 127.0.0.1 -n 8 >NUL

start /LOW java -cp %FO2_JAR% FellOmen_2 --port%2 --rng-seed1 Fo2 --strategy-path%FO2_STR%

:: --rng-seed1
