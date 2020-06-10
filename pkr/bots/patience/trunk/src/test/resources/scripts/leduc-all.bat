set BASEDIR=..\..\..\..\target\dist\
set DECK=%BASEDIR%data\ai.pkr.metastrategy.leduc-he.deck.1.xml 

::%BASEDIR%bin\pkrloggen --help

%BASEDIR%bin\pkrloggen --deck:%DECK% -r:120 leduc-all.cfg 
