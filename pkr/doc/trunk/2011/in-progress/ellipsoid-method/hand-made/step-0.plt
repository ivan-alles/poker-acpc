set samples 29, 29
set isosamples 29, 29
set xlabel "h1" 
set xlabel  offset character -3, -2, 0 font "" textcolor lt -1 norotate
set xrange [ -0.2 : 1.2 ] noreverse nowriteback
set xtics   (0.0, 0.2, 0.4, 0.6, 0.8, 1.0)
set ylabel "h2" 
set ylabel  offset character 3, -2, 0 font "" textcolor lt -1 rotate by 90
set yrange [ -0.2 : 1.2 ] noreverse nowriteback
set ytics   (0.0, 0.2, 0.4, 0.6, 0.8, 1.0)
set zlabel "v" 
set zlabel  offset character -5, 0, 0 font "" textcolor lt -1 norotate
min(a,b) = (a < b) ? a : b
set xyplane at 0
set title ""

set dummy h1, h2

v1(h1, h2) =  3*h1 + 3*h2
v2(h1, h2) =    h1 - 2*h2 + 1
v3(h1, h2) = -2*h1 +   h2 + 1

v0(h1,h2) = min(min(v1(h1,h2), v2(h1,h2)), v3(h1,h2))

#Draw variable bounds
set style line 1  linetype 0 linewidth 3
set style arrow 2 nohead linestyle 1 linecolor rgb "blue"
set arrow from 0, -0.2, 0 to 0, 1.2, 0 arrowstyle 2
set arrow from 1, -0.2, 0 to 1, 1.2, 0 arrowstyle 2
set arrow from -0.2, 0, 0 to 1.2, 0, 0 arrowstyle 2
set arrow from -0.2, 1, 0 to 1.2, 1, 0 arrowstyle 2

ss(h1,h2) = sqrt(0.5*0.5 - (h1-0.5)*(h1-0.5) - (h2-0.5)*(h2-0.5))


#splot v1(h1,h2), v2(h1,h2), v3(h1,h2)
#splot v0(h1,h2), ss(h1,h2) with lines 2, -ss(h1,h2)  with lines 2
splot 'test.dat' 

