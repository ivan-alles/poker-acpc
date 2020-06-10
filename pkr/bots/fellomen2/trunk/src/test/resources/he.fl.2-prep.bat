@echo off
:: Prepares pkr log for verification.

set CONV_PATH=C:\ivan\dev\pkr\meerkat\src\main\py

7z x -y pa.fo2.he.fe.zip 


python %CONV_PATH%\pkrlogfrompa.py pa.fo2.he.fe.txt >pa.fo2.he.fe.2.log



