@echo off
:: Runs a session with ACPC server.

call setenv.bat

:: We have to change directory to bin directory, otherwise there are problems with
:: creating types by assembly name.


pushd %BOTTEST_BIN%

ai.pkr.acpc.server-adapter.1.exe -v --server-addr:ping 192.168.178.51:18791 --bot-class:ai.pkr.bots.patience.Patience;%BOTTEST_BOTBIN%ai.pkr.bots.patience.dll --creation-params:%BOTTEST_ROOT%creation-params.xml

popd
