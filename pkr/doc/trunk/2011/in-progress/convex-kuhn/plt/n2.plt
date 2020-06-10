# Header begin (same for all files) ----------------------------

# h5 -> x, h19 -> y, v -> z

set samples 31, 31
set isosamples 7, 7

set xlabel "h5" 
set xlabel  offset character -3, -2, 0 font "" textcolor lt -1 norotate
set xrange [ 0.0 : 1.0 ] noreverse nowriteback
set xtics   ("0" 0.0, "1/3" 0.3333, "2/3" 0.66667, "1" 1.0)

set ylabel "h19" 
set ylabel  offset character 3, -2, 0 font "" textcolor lt -1 rotate by 90
set yrange [ 0.0000 : 1.0000 ] noreverse nowriteback
set ytics   ("0" 0.0, "1/3" 0.3333, "2/3" 0.66667, "1" 1.0)

set zlabel "v" 
set zlabel  offset character -5, 0, 0 font "" textcolor lt -1 norotate

min(a,b) = (a < b) ? a : b

set dummy h5, h19

set xyplane at 0

# Header end ----------------------------

set title "Node 2" 

v6(h5,h19) =   0.167
v7(h5,h19) =   0.333
v5(h5,h19) = min(v6(h5, h19), v7(h5, h19))

v4(h5,h19) =  0.167

v3(h5,h19) =  v4(h5,h19) + v5(h5,h19)

v9(h5,h19) =  -0.167*(1-h19)
v10(h5,h19) =  0.333*h19 + 0.333
v8(h5,h19) = v9(h5, h19) + v10(h5, h19)


v2(h5,h19) = min(v3(h5, h19), v8(h5, h19))



splot v3(h5, h19), v8(h5, h19), v2(h5, h19) with points


