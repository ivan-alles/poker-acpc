set samples 9, 9
set isosamples 9, 9
set xlabel "h1" 
set xlabel  offset character -3, -2, 0 font "" textcolor lt -1 norotate
set xrange [ -0.2 : 1.2 ] noreverse nowriteback
set xtics   (0.0, 0.2, 0.4, 0.6, 0.8, 1.0)
set ylabel "h7" 
set ylabel  offset character 3, -2, 0 font "" textcolor lt -1 rotate by 90
set yrange [ -0.2 : 1.2 ] noreverse nowriteback
set ytics   (0.0, 0.2, 0.4, 0.6, 0.8, 1.0)
set zlabel "v" 
set zlabel  offset character -5, 0, 0 font "" textcolor lt -1 norotate
min(a,b) = (a < b) ? a : b
set xyplane at 0
set title "Sum of 2 convex functions, illustrates the idea that\n each plane of 1st function is summed with each plane of the 2nd funciton."

f1(x, y) = x 
f2(x, y) = 1-x 
f(x,y) = min(f1(x,y), f2(x,y))

g1(x, y) = y 
g2(x, y) = 1-y 
g(x,y) = min(g1(x,y), g2(x,y))

s(x,y) = f(x,y) + g(x,y)


splot f(x,y), g(x,y), s(x,y)


