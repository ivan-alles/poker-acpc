set ROOT=%CD%\..\..\..\target\dist
set PATH=%ROOT%\Debug;%PATH%

set OUTDIR=%ROOT%\var\tmp

set DeckGenNode=ai.pkr.metastrategy.algorithms.gentree.DeckGenNode

if not exist %OUTDIR% mkdir %OUTDIR%

:: You can goto to the desired game to speed up generation. You can temproarily add an
:: exit statement at the end of the desired game.
:: goto MiniFlBucketizer

:Kuhn

pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:action   -o:%OUTDIR%\kuhn-a.gv --show-expr:s[d].Tree.Nodes[s[d].Node].Round;\nr={1}
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:action   -o:%OUTDIR%\kuhn-a.dat
pkrtree -i:%OUTDIR%\kuhn-a.dat                                 --tree:action   -o:%OUTDIR%\kuhn-a-f.gv

pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:chance -o:%OUTDIR%\kuhn-c.gv 
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:chance   -o:%OUTDIR%\kuhn-c.dat
pkrtree -i:%OUTDIR%\kuhn-c.dat                                 --tree:chance -o:%OUTDIR%\kuhn-c-f.gv
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:chance-player -o:%OUTDIR%\kuhn-c-0.gv -p:0
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:chance-player -o:%OUTDIR%\kuhn-c-1.gv -p:1

pkrtree -i:%OUTDIR%\kuhn-c.dat                               --tree:chance -o:%OUTDIR%\kuhn-c.txt
pkrtree -i:%OUTDIR%\kuhn-c.txt                               --tree:chance -o:%OUTDIR%\kuhn-c-from-txt.dat
pkrtree -i:%OUTDIR%\kuhn-c-from-txt.dat                      --tree:chance -o:%OUTDIR%\kuhn-c-2.txt


pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:strategy -o:%OUTDIR%\kuhn-s-0.gv -p:0
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:strategy -o:%OUTDIR%\kuhn-s-1.gv -p:1
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:strategy -o:%OUTDIR%\kuhn-s-0.dat -p:0
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\kuhn.gamedef.xml  --tree:strategy -o:%OUTDIR%\kuhn-s-1.dat -p:1
pkrtree -i:%OUTDIR%\kuhn-s-0.dat                               --tree:strategy -o:%OUTDIR%\kuhn-s-0-f.gv
pkrtree -i:%OUTDIR%\kuhn-s-1.dat                               --tree:strategy -o:%OUTDIR%\kuhn-s-1-f.gv

pkrtree -i:%OUTDIR%\kuhn-s-0.dat                               --tree:strategy -o:%OUTDIR%\kuhn-s-0.txt
pkrtree -i:%OUTDIR%\kuhn-s-0.txt                               --tree:strategy -o:%OUTDIR%\kuhn-s-0-from-txt.dat
pkrtree -i:%OUTDIR%\kuhn-s-0-from-txt.dat                      --tree:strategy -o:%OUTDIR%\kuhn-s-0-2.txt


:LeducHe

pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\leduc-he.gamedef.xml  --tree:action   -o:%OUTDIR%\leduc-he-a.gv 
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\leduc-he.gamedef.xml  --tree:chance -o:%OUTDIR%\leduc-he-c.gv 
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\leduc-he.gamedef.xml  --tree:chance-player -o:%OUTDIR%\leduc-he-c-0.gv -p:0
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\leduc-he.gamedef.xml  --tree:strategy -o:%OUTDIR%\leduc-he-s-0.gv -p:0

:Ocp

pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\ocp.gamedef.xml  --tree:action   -o:%OUTDIR%\ocp-a.gv 
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\ocp.gamedef.xml  --tree:chance -o:%OUTDIR%\ocp-c.gv 
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\ocp.gamedef.xml  --tree:chance-player -o:%OUTDIR%\ocp-c-0.gv -p:0
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\ocp.gamedef.xml  --tree:strategy -o:%OUTDIR%\ocp-s-0.gv -p:0

:MiniFl

pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\mini-fl.gamedef.xml  --tree:action   -o:%OUTDIR%\mini-fl-a.gv 
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\mini-fl.gamedef.xml  --tree:chance -o:%OUTDIR%\mini-fl-c.gv 
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\mini-fl.gamedef.xml  --tree:chance-player -o:%OUTDIR%\mini-fl-c-0.gv -p:0
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\mini-fl.gamedef.xml  --tree:strategy -o:%OUTDIR%\mini-fl-s-0.gv -p:0


:MicroFl

pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\micro-fl.gamedef.xml  --tree:action   -o:%OUTDIR%\micro-fl-a.gv 
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\micro-fl.gamedef.xml  --tree:chance -o:%OUTDIR%\micro-fl-c.gv 
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\micro-fl.gamedef.xml  --tree:chance-player -o:%OUTDIR%\micro-fl-c-0.gv -p:0
pkrtree -g:%ROOT%\data\ai.pkr.metastrategy\micro-fl.gamedef.xml  --tree:strategy -o:%OUTDIR%\micro-fl-s-0.gv -p:0
