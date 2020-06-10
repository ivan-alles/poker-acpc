# Shows distribution and k-means centers for 2d values.
# values.txt must contain 2d data.
# centers.txt must contain centers of the clusters.

set dummy t,y
set format x "%3.2f"
set format y "%3.2f"
unset key
set parametric
set pointsize 2
set title "HS SD3 AHVO normalized for AA, flop bucket 4" 
set xlabel "HS" 
set xrange [ 0.00000 : 1.00000 ] noreverse nowriteback
set ylabel "SD3" 
set yrange [ 0.0 : 1.0 ] noreverse nowriteback
set zlabel "AHVO" 
set zrange [ 0.0 : 1.0 ] noreverse nowriteback

splot 'AA-4.txt' using 1:2:3 with dots, 'AA-4-centers.txt' using 1:2:3 with points
