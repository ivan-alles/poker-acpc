@echo off
:: Runs pkrserver with the game log converted from PA.

call set-env.bat

start pkrserver ss-verify.he.fl.2.xml 

@echo Press any key to run the clients.
pause

start "Mvb1" java -cp %JAR% ai.pkr.meerkat.MeerkatVerifierBot  --rng-seed1 Mvb1
start "Mvb2" java -cp %JAR% ai.pkr.meerkat.MeerkatVerifierBot  --rng-seed2 Mvb2




