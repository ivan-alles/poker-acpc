@echo off
:: Print records, synopsis:
:: print.bat <pos> <id> 

call setenv.bat

ai.pkr.bots.neytiri.builder -seq-print -position:%1 -node-id:%2 -working-dir:build