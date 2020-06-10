:: Calculate best response for a strategy
:: Parameters
:: %1: iteration count

if not exist dist mkdir dist

set PATH=Release;%PATH%

:: Abstraction for pos 0
set ABSTR0=abstractions\FullGame
:: Abstraction for pos 1
set ABSTR1=abstractions\FullGame


open_cfr_train --abstraction1=%ABSTR0% --abstraction2=%ABSTR1% --output1=dist\eq0.txt --output2=dist\eq1.txt --iterations=%1 

open_cfr_best_response --abstraction=%ABSTR0% --strategy=dist\eq1.txt --player=1 --output=dist\br0.txt

open_cfr_best_response --abstraction=%ABSTR1% --strategy=dist\eq0.txt --player=2 --output=dist\br1.txt



