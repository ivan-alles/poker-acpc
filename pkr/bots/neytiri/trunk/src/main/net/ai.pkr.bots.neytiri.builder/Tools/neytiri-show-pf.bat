@echo off
call set-env.bat

"%ROOT%\CounterFellOmen2.Strategy.exe" -print-neytiri-pf -neytiri:build\Neytiri.Str.xml  >build\Neytiri.PreFlop.txt
"%ROOT%\CounterFellOmen2.Strategy.exe" -show-fo2-act -max-round:0 -show-buckets:1 -fo2-act:build\Neytiri.Str.xml
dot.exe  -Tpdf -o"build\Neytiri.PreFlop-0.pdf" -Kdot build\Neytiri.Str-1.gv  
dot.exe  -Tpdf -o"build\Neytiri.PreFlop-1.pdf" -Kdot build\Neytiri.Str-0.gv
del /q build\Neytiri.Str-0.gv
del /q build\Neytiri.Str-1.gv
