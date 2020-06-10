@echo off
:: Create initial configuration for the solver.
:: Parameters: none
::
call setenv.bat

if exist %OUTDIR% rd /S /Q %OUTDIR%

mkdir %OUTDIR%
mkdir %OUTDIR%\input

pkrtree -g:%GAMEDEF%  --tree:action   -o:%OUTDIR%\input\at.dat
pkrtree -g:%GAMEDEF%  --tree:chance   -o:%OUTDIR%\input\ct.dat
