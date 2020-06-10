:: Parameters
:: %1 number of buckets

set THISDIR=ca\pf\hs

pushd ..\..\..

run heca-pfkm --dim:1 -k:%1 --stages:1000 --use-pocket-counts+ --skip-pocket-names %THISDIR%\hs.txt >%THISDIR%\hs-km-%1.txt

popd