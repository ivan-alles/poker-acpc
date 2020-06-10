:: A simple test script for pkrloggen

set PATH=..\..\..\..\target\dist\Debug

pkrloggen.exe -r:1000 --deck:C:\ai\data\ai.pkr.stdpoker.deck.xml  QQ.cfg --rng-seed:1 -enum-count
pkrloggen.exe -r:1000 --deck:C:\ai\data\ai.pkr.stdpoker.deck.xml  QQ.cfg --rng-seed:1 >QQ.log

pkrloggen.exe -r:104 --deck:C:\ai\data\ai.pkr.stdpoker.deck.xml  1.cfg --enum-count
pkrloggen.exe -r:104 --deck:C:\ai\data\ai.pkr.stdpoker.deck.xml  1.cfg --rng-seed:1 >1.log

pkrloggen.exe -r:2652 --deck:C:\ai\data\ai.pkr.stdpoker.deck.xml  2.cfg --enum-count
pkrloggen.exe -r:2652 --deck:C:\ai\data\ai.pkr.stdpoker.deck.xml  2.cfg --rng-seed:1 >2.log

pkrloggen.exe -r:1326 --deck:C:\ai\data\ai.pkr.stdpoker.deck.xml  3.cfg --enum-count
pkrloggen.exe -r:1326 --deck:C:\ai\data\ai.pkr.stdpoker.deck.xml  3.cfg --rng-seed:1 >3.log