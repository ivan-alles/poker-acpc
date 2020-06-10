# Shows distribution and k-means centers for 2d values.
# values.txt must contain 2d data.
# centers.txt must contain centers of the clusters.

set dummy t,y
set format x "%3.2f"
set format y "%3.2f"
unset key
set parametric
set style data circles
set style circle radius 0.0005
set title "HS SD 3 normalized for 87s, flop bucket 5, turn bucket 3" 
set xlabel "value" 
set xrange [ 0.00000 : 1.00000 ] noreverse nowriteback
set ylabel "random offset" 
set yrange [ 0.0 : 1.0 ] noreverse nowriteback


plot 'sd3-norm-87s-5-3.txt' using 1:(rand(0)*0.8 + 0.1), 'sd3-norm-87s-5-3-centers.txt' using 1:2:(0.005) with circles
