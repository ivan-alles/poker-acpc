@echo off
:: Prints a list of preflop abstract card for each pocket kind.
:: The list is sorted by pocke kinds.
:: Parameters:
:: %1 chance abstraction

call setenv
run hecainfo %1 --preflop-ranges- @pk-to-ac-params.txt 