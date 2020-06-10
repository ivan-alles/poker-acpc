@echo off
call setenv.bat

hecamcgen.exe --rng-seed:1 --samples-count:0,5000,5000,200  ca-sd-test.xml >ca-sd-test-gen.log

run heca-clustertree-vis.exe ca-sd-test.dat 



