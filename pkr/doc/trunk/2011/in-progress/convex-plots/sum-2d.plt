# Header begin (same for all files) ----------------------------

# h5 -> x, h19 -> y, v -> z

set samples 31, 31
set isosamples 7, 7

set xlabel "x" 
set xlabel  offset character -3, -2, 0 font "" textcolor lt -1 norotate
set xrange [ 0.0 : 1.0 ] noreverse nowriteback

set ylabel "y" 
set ylabel  offset character 3, -2, 0 font "" textcolor lt -1 rotate by 90
set yrange [ 0.0000 : 1.5000 ] noreverse nowriteback


min(a,b) = (a < b) ? a : b

set dummy x

# Header end ----------------------------

set title "Sum of 2 convex functions is a convex function." 

g1(x) = 2*x
g2(x) = 1-x
f1(x) = min(g1(x), g2(x))

h1(x) = 2*(1-x)
h2(x) = 1-(1-x)
f2(x) = 1.3*min(h1(x), h2(x))


plot f1(x), f2(x), f1(x) + f2(x)


