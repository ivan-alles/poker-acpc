set ROOT=%CD%\..\..
set PATH=%ROOT%\Debug;%ROOT%\bin;%PATH%

set OUTDIR=%CD%

pkrtree.1 -g:%ROOT%\data\ai.pkr.metastrategy.kuhn.gamedef.1.xml --tree:player -p:1 -o:%OUTDIR%\tree-p-1.gv
pkrtree.1 -g:%ROOT%\data\ai.pkr.metastrategy.kuhn.gamedef.1.xml --tree:player -p:1 -o:%OUTDIR%\tree-p-1.xml --out-format:xml
pkrtree.1 -g:%ROOT%\data\ai.pkr.metastrategy.kuhn.gamedef.1.xml --tree:player -p:0 -o:%OUTDIR%\tree-p-0.gv
pkrtree.1 -g:%ROOT%\data\ai.pkr.metastrategy.kuhn.gamedef.1.xml --tree:player -p:0 -o:%OUTDIR%\tree-p-0.xml --out-format:xml
