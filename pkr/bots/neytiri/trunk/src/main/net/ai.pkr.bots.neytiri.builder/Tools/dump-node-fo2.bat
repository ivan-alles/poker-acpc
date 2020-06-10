@echo off
call set-env.bat

"%ROOT%\CounterFellOmen2.Strategy.exe" -dump-node  -fo2-act:build\Fo2.At.xml -node-id:%1
