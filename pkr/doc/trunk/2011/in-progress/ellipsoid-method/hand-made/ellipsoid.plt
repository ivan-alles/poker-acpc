set parametric
splot cos(u)*cos(v),2*sin(u)*cos(v),3*sin(v)
set contour base
set grid xtics ytics ztics
#unset key
#set term svg
#set output "Gnuplot_ellipsoid.svg"
#replot
