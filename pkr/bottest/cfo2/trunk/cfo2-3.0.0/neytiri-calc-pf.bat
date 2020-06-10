@echo off
call setenv.bat

if exist build\neytiri.xml goto update


move /Y build\opp-at.xml build\neytiri.xml

:update

ai.pkr.bots.neytiri.builder -calc-preflop-enum -pockets:"Tc 9c" -position:0 -neytiri:build\neytiri.xml -game-def:%GAME_DEF%



