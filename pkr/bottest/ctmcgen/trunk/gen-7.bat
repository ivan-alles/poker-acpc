@echo off
:: Starts 7 instances of gen.bat.
:: Parameters: 
:: %1 chance abstraction 1
:: %2 chance abstraction 2

call gen %1 %2
timeout 5

call gen %1 %2
timeout 5

call gen %1 %2
timeout 5

call gen %1 %2
timeout 5

call gen %1 %2
timeout 5

call gen %1 %2
timeout 5

call gen %1 %2
timeout 5


