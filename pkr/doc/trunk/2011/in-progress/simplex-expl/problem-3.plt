set samples 31, 31
set isosamples 31, 31

set xlabel "x1" 
set xlabel  offset character -3, -2, 0 font "" textcolor lt -1 norotate
set xrange [ 0.0 : 2.0 ] noreverse nowriteback

set ylabel "x2" 
set ylabel  offset character 3, -2, 0 font "" textcolor lt -1 rotate by 90
set yrange [ 0.0000 : 2.0000 ] noreverse nowriteback

set zlabel "s1" 
set zlabel  offset character -5, 0, 0 font "" textcolor lt -1 norotate
set zrange [ 0.0000 : 2.0000 ] noreverse nowriteback

set xyplane at 0

min(a,b) = (a < b) ? a : b

set dummy x1, x2


set title "Simplex slack var for c1: x1 + 2*x2 <= 2" 

c1(x1,x2) =  2 - (x1 + 2*x2)


splot  c1(x1,x2)


