# Header begin (same for all files) ----------------------------

# h5 -> x, h19 -> y, v -> z

set samples 19, 19
set isosamples 19, 19

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

set title "Node 0" 

#------------ Node 2 -----------

v6(h5,h19) =   0.167
v7(h5,h19) =   0.333
v5(h5,h19) = min(v6(h5, h19), v7(h5, h19))

v4(h5,h19) =  0.167

v3(h5,h19) =  v4(h5,h19) + v5(h5,h19)

v9(h5,h19) =  -0.167*(1-h19)
v10(h5,h19) =  0.333*h19 + 0.333
v8(h5,h19) = v9(h5, h19) + v10(h5, h19)


v2(h5,h19) = min(v3(h5, h19), v8(h5, h19))

#------------ Node 12 -----------

v16(h5,h19) =   0.167*h5 + 0.167
v17(h5,h19) = - 0.333*h5 + 0.333
v15(h5,h19) = min(v16(h5, h19), v17(h5, h19))

v14(h5,h19) = - 0.167*(1-h5)

v13(h5,h19) =  v14(h5,h19) + v15(h5,h19)

v19(h5,h19) =  -0.167
v20(h5,h19) =   0.333
v18(h5,h19) = v19(h5, h19) + v20(h5, h19)

v12(h5,h19) = min(v13(h5, h19), v18(h5, h19))

#------------ Node 22 -----------

v26(h5,h19) =   0.167*h5 
v27(h5,h19) = - 0.333*h5
v25(h5,h19) = min(v26(h5, h19), v27(h5, h19))

v24(h5,h19) = - 0.167*(1-h5) - 0.167

v23(h5,h19) =  v24(h5,h19) + v25(h5,h19)

v29(h5,h19) =  -0.167 - 0.167*(1-h19)
v30(h5,h19) =  -0.333*h19
v28(h5,h19) = v29(h5, h19) + v30(h5, h19)

v22(h5,h19) = min(v23(h5, h19), v28(h5, h19))

#------------ Total ---------------

v0(h5,h19) = v2(h5, h19) + v12(h5, h19) + v22(h5, h19) 

#------------ "Active" plane in 0, 0.5 ------------


act(h5,h19) = 0.333*h5 - 0.167*h19

set style arrow 1 lw 5

set arrow 1 from 0,0.5,- 0.167*0.5 to 0,0.6,- 0.167*(0.6) arrowstyle 1

splot v0(h5, h19), act(h5, h19) with points





