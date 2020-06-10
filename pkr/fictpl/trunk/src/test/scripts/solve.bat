@echo off
:: Solves a game by fictitios play
:: Parameters: none
::
call setenv.bat

fictpl.1  --epsilons:0.1,0.05 --snapshot-count:3 --iteration-verbosity:10000 --epsilon-log-threshold:0.1  --chance-tree:%OUTDIR%\input\ct.dat --action-tree:%OUTDIR%\input\at.dat -o:%OUTDIR%




