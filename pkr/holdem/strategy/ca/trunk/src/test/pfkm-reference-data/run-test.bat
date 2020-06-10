@echo off
:: Runs pfkm with test data
:: Parameters: 
:: none

set CONFIG=Debug
set PLATFORM=win32

set PATH=..\..\..\target\dist\%CONFIG%;..\..\..\target\dist\bin;..\..\..\target\dist\bin\%PLATFORM%;%PATH%

:: Data set 1 (1d HS values)

kmltest.exe -i hs-kmltest.in -o  hs-kmltest.out
heca-pfkm.1 --dim:1 --use-pocket-counts- --stages:1000 -k:10  hs-input.txt >hs-output.txt


kmltest.exe -i hssd3-kmltest.in -o  hssd3-kmltest.out
heca-pfkm.1 --dim:2 --use-pocket-counts+ --stages:1000 -k:10  hssd3-input.txt >hssd3-output.txt


