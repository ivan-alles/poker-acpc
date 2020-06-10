@echo off
:: Finds best response
:: Parameters: 
:: %1 action tree
:: %2 chance tree
:: %3 hero position
:: %4 opp strategy
call setenv.bat

set ATREE=%1
set CTREE=%2

pkrbr  --chance-tree:%CTREE% --action-tree:%ATREE% --hero-pos:%3 --opp-strategy:%4





