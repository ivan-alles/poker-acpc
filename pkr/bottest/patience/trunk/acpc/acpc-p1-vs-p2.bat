@echo off
:: Runs Patience eq vs sbr to verify the play with ACPC server.

call setenv.bat

:: We have to change directory to bin directory, otherwise there are problems with
:: creating types by assembly name.


pushd %BOTTEST_BIN%

start ai.pkr.acpc.server-adapter.exe -v  --server-addr:192.168.178.51:18791 --bot-class:ai.pkr.bots.patience.Patience;%BOTTEST_BOTBIN%ai.pkr.bots.patience.dll --creation-params:%BOTTEST_ROOT%creation-params-1.xml

ai.pkr.acpc.server-adapter.exe -v --server-addr:192.168.178.51:18374 --bot-class:ai.pkr.bots.patience.Patience;%BOTTEST_BOTBIN%ai.pkr.bots.patience.dll --creation-params:%BOTTEST_ROOT%creation-params-2.xml


popd
