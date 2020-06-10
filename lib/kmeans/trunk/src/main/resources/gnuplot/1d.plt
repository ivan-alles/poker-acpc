# Shows distribution for 1d values.
# values.txt must contain 1d data.

set dummy t,y
set format x "%3.2f"
set format y "%3.2f"
unset key
set parametric
set style data dots
set title "Distribution of 1d values" 
set xlabel "value" 
set xrange [ 0.00000 : 1.00000 ] noreverse nowriteback
set ylabel "random offset" 
set yrange [ 0.00000 : 1.00000 ] noreverse nowriteback

plot 'values.txt' using 1:(rand(0)*0.9)