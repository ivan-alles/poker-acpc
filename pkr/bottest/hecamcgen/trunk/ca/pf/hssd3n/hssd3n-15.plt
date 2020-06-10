# Shows distribution of HE pockets and k-means centers for 2d values.
# values.txt must contain pocket names and 2d values.
# centers.txt must contain centers of the clusters.

set dummy t,y
set format x "%3.2f"
set format y "%3.2f"
unset key

set style data circles
set style circle radius 0.0005
set title "15 preflop buckets by normalized HS SD3." 
set xlabel "value" 
set xrange [ -.05 : 1.05 ] noreverse nowriteback
set ylabel "random offset" 
set yrange [ -.05 : 1.05 ] noreverse nowriteback


plot  'hssd3n.txt' using  2:3 with circles, '' using  2:3:1 with labels font "Helvetica,7" left offset 1,0, 'centers-15.txt' using 1:2:(0.005) with circles
