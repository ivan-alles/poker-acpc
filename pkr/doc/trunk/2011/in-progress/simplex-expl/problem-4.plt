set samples 31, 31
set isosamples 31, 31

set xlabel "x1" 
set xlabel  offset character -3, -2, 0 font "" textcolor lt -1 norotate
set xrange [ 0.0 : 10.0 ] noreverse nowriteback

set ylabel "x2" 
set ylabel  offset character 3, -2, 0 font "" textcolor lt -1 rotate by 90
set yrange [ 0.0000 : 10.0000 ] noreverse nowriteback



set dummy x1


set title "Constraints c1: x1/8 + x2 <= 10; c2: x1+2/9x2 <= 10" 

c1(x1) =  10-x1/8
c2(x1) =  45-x1*9/2


plot  c1(x1), c2(x1)
