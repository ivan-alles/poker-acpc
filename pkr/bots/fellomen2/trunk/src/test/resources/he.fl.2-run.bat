@echo off
:: Runs pkrserver with the game log converted from PA.

call set-env.bat

start pkrserver ss-verify.he.fl.2.xml 

@echo Press any key to run the clients.
pause

start "Fo21" java -cp %JAR% FellOmen_2  --rng-seed1 Fo21 
start "Fo22" java -cp %JAR% FellOmen_2  --rng-seed2 Fo22 



