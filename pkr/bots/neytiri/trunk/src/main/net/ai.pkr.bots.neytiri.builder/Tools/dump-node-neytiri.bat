@echo off
call set-env.bat

"%ROOT%\CounterFellOmen2.Strategy.exe" -dump-node  -fo2-act:build\Neytiri.Str.xml -node-id:%1
