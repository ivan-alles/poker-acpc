@echo off

call setenv.bat

pushd %BOTTEST_BIN%

start ag --game-def:%BOTTEST_BIN%\..\data\ai.pkr.holdem.gamedef.2/holdem-gd-fl-2.xml --bot-class:ai.pkr.bots.patience.Patience;%BOTTEST_BOTBIN%ai.pkr.bots.patience.dll --creation-params:%BOTTEST_ROOT%\agentgui-creation-params.xml

popd

