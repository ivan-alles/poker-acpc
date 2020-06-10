@echo off
:: Print neytiry preflop tree:
:: neytiri-pftree.bat <pf-bucket>
call setenv.bat

set TREE=build\neytiri.xml
::set TREE=C:\bottest\cfo2\dist\neytiri.xml

ai.pkr.bots.neytiri.builder -show-opp-act -opp-act:%TREE% -show-property:s[d].Node.PreflopValues==null?0:s[d].Node.PreflopValues[%1];\nPf:{1:0.0000} -max-round:0