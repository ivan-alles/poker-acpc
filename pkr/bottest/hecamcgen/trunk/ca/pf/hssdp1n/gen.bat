:: Parameters
:: %1 number of buckets

set THISDIR=ca\pf\hssd1n

pushd ..\..\..

run heca-pfkm --dim:2 -k:%1 --stages:1000 --use-pocket-counts+ --skip-pocket-names %THISDIR%\hssd1n.txt >%THISDIR%\hssd1n-km-%1.txt

popd