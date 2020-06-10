@echo off
:: Solves a game by fictitios play
:: Parameters: 
:: %1 output directory
call setenv.bat

set OUTDIR=%1
set CTREE=%OUTDIR%\input\ct.dat
set ATREE=%OUTDIR%\input\at.dat
set EPS=0.040,0.020,0.010,0.005

fictpl  --epsilons:%EPS% --thread-count:0 --snapshot-count:4 --iteration-verbosity:1000 --epsilon-log-threshold:0.9  --chance-tree:%CTREE% --action-tree:%ATREE% -o:%OUTDIR%




