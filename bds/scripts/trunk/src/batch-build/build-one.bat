::@echo off

set DIR=%1

if exist %DIR% goto DirExists


mkdir %DIR%
SET URL=%DIR:\=/%

svn co http://ai.dyndns.biz/svn/aidev/ai/%URL%/trunk %DIR%

:DirExists


echo ---------- Building directory %DIR% ---------- 
pushd %DIR%
svn up
call mvn %2 %3 %4 %5 %6 %7
popd
echo ---------- Building directory %DIR% finished ----------
