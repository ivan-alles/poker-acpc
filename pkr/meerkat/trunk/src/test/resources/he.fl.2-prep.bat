@echo off
:: Prepares pkr log for verification.


7z x -y pa.mv.he.fe.zip 

..\..\main\py\pkrlogfrompa.py pa.mv.he.fe.txt >pa.mv.he.fe.2.log



