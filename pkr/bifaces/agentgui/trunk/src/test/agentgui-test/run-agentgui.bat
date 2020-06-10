call set-env.bat

set EXE=ag.1.exe

:: Copy agentgui to target\dist and run from there, so that all dlls can be loaded.

copy %DEV_DIST%\%CFG%\%EXE% %BOTTEST_AIROOT%\bin

pushd %BOTTEST_AIROOT%\bin

%EXE% --debugger-launch --game-def:%BOTTEST_AIROOT%\data\ai.pkr.holdem.gamedef.2/holdem-gd-fl-2.xml --bot-class:ai.pkr.bots.patience.Patience;%BOTTEST_BOTBIN%ai.pkr.bots.patience.dll --creation-params:%BOTTEST_ROOT%\creation-params.xml

popd

