@echo off
:: Parameters:
:: %* paths to logfiles

run pkrlogstat -t --report-class:ai.pkr.holdem.strategy.core.PocketReport,ai.pkr.holdem.strategy.core.2 --report-param:HeroName=Patience %*