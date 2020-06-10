# Header begin (same for all files) ----------------------------


set samples 31, 131
set isosamples 31, 131

set xlabel "h0" 
set xlabel  offset character -3, -2, 0 font "" textcolor lt -1 norotate
set xrange [ -1.0 : 1.0 ] noreverse nowriteback

set ylabel "h1" 
set ylabel  offset character 3, -2, 0 font "" textcolor lt -1 rotate by 90
set yrange [ -1.0000 : 1.0000 ] noreverse nowriteback


set zlabel "v" 
set zlabel  offset character -5, 0, 0 font "" textcolor lt -1 norotate
set zrange [ 0.0000 : 1.0000 ] noreverse nowriteback


set xyplane at 0

set dummy h0, h1

# To draw a cross in a center
cdelta = 0.01

# ----------------- AABB P0 --------------------

s0_0 = 0.25
s0_1 = 0.6
s0_2 = 0.25

x0_0 = 0.25
x0_1 = -0.25
x0_2 = 0.25

set label "x0_0" at x0_0 + 2*cdelta, x0_1 + 2*cdelta, x0_2 + 2*cdelta

# Center 
set style line 1  linewidth 2
set style arrow 1 nohead linestyle 1 linecolor rgb "red"
set arrow from x0_0-cdelta, x0_1, x0_2 to x0_0+cdelta, x0_1, x0_2 arrowstyle 1
set arrow from x0_0, x0_1-cdelta, x0_2 to x0_0, x0_1+cdelta, x0_2 arrowstyle 1
set arrow from x0_0, x0_1, x0_2-cdelta to x0_0, x0_1, x0_2+cdelta arrowstyle 1


# AABB
set style line 2  linetype -1 linewidth 8
set style arrow 2 nohead linestyle 2 linecolor rgb "red"

set arrow from x0_0-s0_0, x0_1-s0_1, x0_2-s0_2 to x0_0+s0_0, x0_1-s0_1, x0_2-s0_2 arrowstyle 2
set arrow from x0_0-s0_0, x0_1-s0_1, x0_2-s0_2 to x0_0-s0_0, x0_1-s0_1, x0_2+s0_2 arrowstyle 2
set arrow from x0_0-s0_0, x0_1-s0_1, x0_2+s0_2 to x0_0+s0_0, x0_1-s0_1, x0_2+s0_2 arrowstyle 2
set arrow from x0_0+s0_0, x0_1-s0_1, x0_2-s0_2 to x0_0+s0_0, x0_1-s0_1, x0_2+s0_2 arrowstyle 2
set arrow from x0_0-s0_0, x0_1+s0_1, x0_2-s0_2 to x0_0+s0_0, x0_1+s0_1, x0_2-s0_2 arrowstyle 2
set arrow from x0_0-s0_0, x0_1+s0_1, x0_2-s0_2 to x0_0-s0_0, x0_1+s0_1, x0_2+s0_2 arrowstyle 2
set arrow from x0_0-s0_0, x0_1+s0_1, x0_2+s0_2 to x0_0+s0_0, x0_1+s0_1, x0_2+s0_2 arrowstyle 2
set arrow from x0_0+s0_0, x0_1+s0_1, x0_2-s0_2 to x0_0+s0_0, x0_1+s0_1, x0_2+s0_2 arrowstyle 2
set arrow from x0_0-s0_0, x0_1-s0_1, x0_2-s0_2 to x0_0-s0_0, x0_1+s0_1, x0_2-s0_2 arrowstyle 2
set arrow from x0_0+s0_0, x0_1-s0_1, x0_2-s0_2 to x0_0+s0_0, x0_1+s0_1, x0_2-s0_2 arrowstyle 2
set arrow from x0_0-s0_0, x0_1-s0_1, x0_2+s0_2 to x0_0-s0_0, x0_1+s0_1, x0_2+s0_2 arrowstyle 2
set arrow from x0_0+s0_0, x0_1-s0_1, x0_2+s0_2 to x0_0+s0_0, x0_1+s0_1, x0_2+s0_2 arrowstyle 2

# ----------------- AABB P1 --------------------

s1_0 = 0.25
s1_1 = (s0_1 + x0_1)/2
s1_2 = 0.25

x1_0 = 0.25
x1_1 = s1_1
x1_2 = 0.25

set label "x1_0" at x1_0 + 2*cdelta, x1_1 + 2*cdelta, x1_2 + 2*cdelta

# Center 
set style line 3  linetype -1 linewidth 2
set style arrow 3 nohead linestyle 3 linecolor rgb "blue"
set arrow from x1_0-cdelta, x1_1, x1_2 to x1_0+cdelta, x1_1, x1_2 arrowstyle 3
set arrow from x1_0, x1_1-cdelta, x1_2 to x1_0, x1_1+cdelta, x1_2 arrowstyle 3
set arrow from x1_0, x1_1, x1_2-cdelta to x1_0, x1_1, x1_2+cdelta arrowstyle 3


# AABB
set style line 4  linetype -1 linewidth 2
set style arrow 4 nohead linestyle 4 linecolor rgb "blue"

set arrow from x1_0-s1_0, x1_1-s1_1, x1_2-s1_2 to x1_0+s1_0, x1_1-s1_1, x1_2-s1_2 arrowstyle 4
set arrow from x1_0-s1_0, x1_1-s1_1, x1_2-s1_2 to x1_0-s1_0, x1_1-s1_1, x1_2+s1_2 arrowstyle 4
set arrow from x1_0-s1_0, x1_1-s1_1, x1_2+s1_2 to x1_0+s1_0, x1_1-s1_1, x1_2+s1_2 arrowstyle 4
set arrow from x1_0+s1_0, x1_1-s1_1, x1_2-s1_2 to x1_0+s1_0, x1_1-s1_1, x1_2+s1_2 arrowstyle 4
set arrow from x1_0-s1_0, x1_1+s1_1, x1_2-s1_2 to x1_0+s1_0, x1_1+s1_1, x1_2-s1_2 arrowstyle 4
set arrow from x1_0-s1_0, x1_1+s1_1, x1_2-s1_2 to x1_0-s1_0, x1_1+s1_1, x1_2+s1_2 arrowstyle 4
set arrow from x1_0-s1_0, x1_1+s1_1, x1_2+s1_2 to x1_0+s1_0, x1_1+s1_1, x1_2+s1_2 arrowstyle 4
set arrow from x1_0+s1_0, x1_1+s1_1, x1_2-s1_2 to x1_0+s1_0, x1_1+s1_1, x1_2+s1_2 arrowstyle 4
set arrow from x1_0-s1_0, x1_1-s1_1, x1_2-s1_2 to x1_0-s1_0, x1_1+s1_1, x1_2-s1_2 arrowstyle 4
set arrow from x1_0+s1_0, x1_1-s1_1, x1_2-s1_2 to x1_0+s1_0, x1_1+s1_1, x1_2-s1_2 arrowstyle 4
set arrow from x1_0-s1_0, x1_1-s1_1, x1_2+s1_2 to x1_0-s1_0, x1_1+s1_1, x1_2+s1_2 arrowstyle 4
set arrow from x1_0+s1_0, x1_1-s1_1, x1_2+s1_2 to x1_0+s1_0, x1_1+s1_1, x1_2+s1_2 arrowstyle 4


# ----------------- Plane --------------------

cut(h0, h1) = 50*h1

splot cut(h0, h1) with lines 2


