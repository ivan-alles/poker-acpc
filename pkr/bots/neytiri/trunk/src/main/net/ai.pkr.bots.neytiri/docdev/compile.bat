@echo off
set PATH=..\..\..\..\..\target\dist\Debug;..\..\..\..\..\target\dist\bin;%PATH%
set GAME_DEF=..\doc\gamedef.xml
set GAME_DEF_2=..\doc\gamedef2.xml
set BUCK=..\doc\bucketizer.xml

if not exist build mkdir build
if not exist dist mkdir dist

:: Delete old action tree to force creation of a new one

if exist build\opp-at-1.xml del build\opp-at-1.xml
if exist build\opp-at-2.xml del build\opp-at-2.xml

:: Process game log for gamelog-1

ai.pkr.bots.neytiri.builder -process-logs -logs:gamelog-1.txt -o:FellOmen2 -opp-act:build\opp-at-1.xml -game-def:%GAME_DEF% -bucketizer:%BUCK%


:: Show empty action tree

ai.pkr.bots.neytiri.builder -show-opp-act -opp-act:build\opp-at-1.xml 
dot.exe  -Tgif -o"build\opp-at-1-0.gif" -Kdot build\opp-at-1-0.gv
dot.exe  -Tpdf -o"build\opp-at-1-0.pdf" -Kdot build\opp-at-1-0.gv


:: Show action tree with buckets

ai.pkr.bots.neytiri.builder -show-opp-act -opp-act:build\opp-at-1.xml -show-buckets:4
dot.exe  -Tgif -o"build\opp-at-1-0-b.gif" -Kdot build\opp-at-1-0.gv 
dot.exe  -Tpdf  -o"build\opp-at-1-0-b.pdf"  -Kdot build\opp-at-1-0.gv 



:: Show empty action tree for GameDef-2

type NUL > empty.log
ai.pkr.bots.neytiri.builder -process-logs -logs:empty.log -o:FellOmen2 -opp-act:build\opp-at-2.xml -game-def:%GAME_DEF_2% -bucketizer:%BUCK%
del empty.log

ai.pkr.bots.neytiri.builder -show-opp-act -opp-act:build\opp-at-2.xml 
dot.exe  -Tgif -o"build\opp-at-2-0.gif" -Kdot build\opp-at-2-0.gv
dot.exe  -Tpdf -o"build\opp-at-2-0.pdf" -Kdot build\opp-at-2-0.gv







