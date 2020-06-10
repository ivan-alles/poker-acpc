:: Starts pkrserver and FellOmen2 
:: Parameters:
:: %1 session suite configuration file
:: %2 TCP Port

call set-env.bat


start pkrserver.exe -p:%2 -l:logs %1

:: A poor man wait.
ping 127.0.0.1 -n 4 >NUL

:: Params:
:: --rng-seed1

start java -cp %JAR% FellOmen_2 --port%2 --rng-seed1 Fo2


