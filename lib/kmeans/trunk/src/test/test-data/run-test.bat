set CONFIG=Debug
set PLATFORM=win32
set DIFF=compare.exe

set PATH=..\..\..\target\dist\%CONFIG%\%PLATFORM%;..\..\..\target\dist\bin;%PATH%


kmltest < test0.in > test0.tmp
%DIFF% test0.save test0.tmp 

kmltest < test1.in > test1.tmp
%DIFF% test1.save test1.tmp 

kmltest < test2.in > test2.tmp
%DIFF% test2.save test2.tmp 

kmltest < test3.in > test3.tmp
%DIFF% test3.save test3.tmp 

kmltest < test4.in > test4.tmp
%DIFF% test4.save test4.tmp 

kmltest < test5.in > test5.tmp
%DIFF% test5.save test5.tmp 

kmltest < test6.in > test6.tmp
%DIFF% test6.save test6.tmp 

kmltest < test7.in > test7.tmp
%DIFF% test7.save test7.tmp 

