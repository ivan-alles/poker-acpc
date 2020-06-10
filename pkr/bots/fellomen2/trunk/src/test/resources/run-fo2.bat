@echo off
:: Starts FellOmen2 
:: Parameters:
:: %1 Name
:: %2 TCP Port
:: %3 RNG seed

call setenv.bat

start /LOW java -cp %FO2_JAR% FellOmen_2 --port%2 --rng-seed%3 --strategy-path%FO2_STR% %1
