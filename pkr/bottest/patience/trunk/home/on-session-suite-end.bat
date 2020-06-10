:: Is called on session suite end.
:: Parameters:
:: %1 log file.

exit

set LOG=%1
set DIR=%~dp0
set ARCH=%LOG%.7z

7z a %ARCH% %LOG%

call email ivan_alles@yahoo.de %0 -attach %ARCH%

del %ARCH%

::pause