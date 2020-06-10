@echo off
:: Parameters:
:: %* paths to logfiles

run pkrlogstat --report-class:ai.pkr.luck.HeHsDeviationReport,ai.pkr.luck.1  --report-param:HeroName=Agent --report-param:PrintHeroChartData=true %*