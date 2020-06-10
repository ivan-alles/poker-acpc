set samples 31, 31
set isosamples 31, 31

set xlabel "x1" 
set xlabel  offset character -3, -2, 0 font "" textcolor lt -1 norotate
set xrange [ 0.0 : 6.0 ] noreverse nowriteback

set ylabel "s1" 
set ylabel  offset character 3, -2, 0 font "" textcolor lt -1 rotate by 90
set yrange [ 0.0000 : 6.0000 ] noreverse nowriteback

set zlabel "s2" 
set zlabel  offset character -5, 0, 0 font "" textcolor lt -1 norotate
set zrange [ 0.0000 : 6.0000 ] noreverse nowriteback

set xyplane at 0

min(a,b) = (a < b) ? a : b

set dummy x1, s1


set title "Simplex slack vars for c1: x1 < 3; c2: x1 < 5 after step 1" 

c1(x1,s1) =  (3-x1-s1)*20
c2(x1,s1) =  5-x1
c2_1(x1,s1) =  2+s1


splot  c1(x1,s1),c2(x1,s1),c2_1(x1,s1)


