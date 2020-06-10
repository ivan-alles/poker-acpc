@echo off
:: Displays a public key from an *.snk file.


sn -p %1 key.tmp
sn -tp key.tmp
del key.tmp

