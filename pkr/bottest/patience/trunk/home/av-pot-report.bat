@echo off
:: Parameters:
:: %* paths to logfiles

run pkrlogstat --report-class:ai.pkr.botuse.logreports.AveragePotReport,ai.pkr.botuse.logreports.1 --report-param:NoFlopNoDrop=true --report-param:PotCap=1.33 %*