@echo off
:: Compares log converted from PA with the pkrserver log (must be copied here under the name pkr.log).

call set-env.bat

pkrlogcmp pa.mv.he.fe.2.log pkr.log



