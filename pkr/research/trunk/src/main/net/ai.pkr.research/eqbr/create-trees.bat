set ROOT=%CD%\..\..
set PATH=%ROOT%\Debug;%ROOT%\bin;%PATH%

set OUTDIR=%CD%

pkrtree.1     -g:eq-br-gd.xml  -tree:game        -suitless -o:%OUTDIR%\tree-g.gv
pkrtree.1     -g:eq-br-gd.xml  -tree:player -p:0 -suitless -o:%OUTDIR%\tree-p-0.gv
pkrtree.1     -g:eq-br-gd.xml  -tree:player -p:1 -suitless -o:%OUTDIR%\tree-p-1.gv
pkrtree.1 xml -g:eq-br-gd.xml  -tree:player -p:1 -suitless -o:%OUTDIR%\tree-p-1.xml
