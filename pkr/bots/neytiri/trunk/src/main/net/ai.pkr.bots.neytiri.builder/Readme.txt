COMMAND LINES:

Show opponent action tree:
-show-opp-act -opp-act:c:\bottest\cfo2\build\opp-at.xml -show-pockets:AcAd

Update:
-process-logs -logs:c:\bottest\cfo2\probe-logs -o:Fo2 -opp-act:c:\bottest\cfo2\build\opp-at.xml -game-def:c:\ai\data\ai.pkr.holdem.gd.fl.2.1.xml -bucketizer:c:\bottest\cfo2\bucketizer.xml

Apply monte-carlo:
-monte-carlo -mc-count:100 -neytiri:c:\bottest\cfo2\build\neytiri.xml


//Show Neytiri preflop strategy:
//-show-fo2-act -fo2-act:c:\temp\cfo2\Neytiry.PreflopStr.xml -show-pockets:AcAdKcKd3c3d2c2d7c6c7c2d

Print Neytiry preflop info:
-print-neytiri-pf -neytiri:c:\bottest\cfo2\build\neytiri.xml  >c:\bottest\cfo2\build\neytiri-pf.txt

Dump node: Neytiri
-dump-node -opp-act:c:\bottest\cfo2\build\neytiri.xml  -node-id:0.4


Doc process log 1;
-process-logs -o:FellOmen2 -logs:..\..\..\src\main\net\ai.pkr.bots.neytiri\docdev\gamelog-1.txt -opp-act:..\..\..\src\main\net\ai.pkr.bots.neytiri\docdev\build\opp-at-1.xml -game-def:..\..\..\src\main\net\ai.pkr.bots.neytiri\doc\gamedef.xml -bucketizer:..\..\..\src\main\net\ai.pkr.bots.neytiri\doc\bucketizer.xml


