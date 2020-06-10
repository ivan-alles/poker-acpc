@echo off
:: Visualize a tree.
:: Parameters: 
:: %1 file name
call set-env.bat

pkrtree --tree:chance -o:%1.gv -i:%1




