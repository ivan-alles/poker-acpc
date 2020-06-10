@echo off
call set-env.bat

"%ROOT%\CounterFellOmen2.Strategy.exe" -show-fo2-act -max-round:4 -fo2-act:build\Fo2.At.xml

::dot.exe  -Tpdf -o"build\Fo2.At-0.pdf" -Kdot build\Fo2.At-0.gv  
::dot.exe  -Tpdf -o"build\Fo2.At-1.pdf" -Kdot build\Fo2.At-1.gv  

