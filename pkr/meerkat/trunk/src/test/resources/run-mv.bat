@echo off
:: Runs a Meerkat Verifier
:: Parameters:
:: %1 Name suffix

call set-env.bat

java -cp %JAR% ai.pkr.meerkat.MeerkatVerifierBot --debug --rng-seed%1 Mvb%1

                        
