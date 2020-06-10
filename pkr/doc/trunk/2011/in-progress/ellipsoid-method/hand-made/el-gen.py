'''
Generates a datafile for gnuplot describing an ellipsoid

'''

import sys

x0 = [0.5, 0.5, 0]

P = [[ 3,    3,     0],
     [ 3,    6,     0],
     [ 0,    0,     8]]


epsilon = 0.1000000000000000
step = 0.05

x0_range = [-1, 1]
x1_range = [-1, 1]
x2_range = [-1, 1]



def mult2(A, x):
   y = [0, 0, 0]
   for i in range(0,3):
      for j in range(0,3):
         y[i] += x[j] * A[j][i]
   m = 0
   for i in range(0,3):
      m += x[i] * y[i]
   return m


x = [0,0,0]

x[0] = x0_range[0] 
while x[0] <= x0_range[1]:
  x[1] = x1_range[0] 
  while x[1] <= x1_range[1]:
    x[2] = x2_range[0] 
    while x[2] <= x2_range[1]:
      r = mult2(P, x)
      if abs(r-1) < epsilon:
        print ( x0[0] + x[0], x0[1] + x[1], x0[2] + x[2] )
        #x1 = mult(T, x)
        #print ( x0[0] + x1[0], x0[1] + x1[1], x0[2] + x1[2] )
      x[2] += step
    x[1] += step         
  x[0] += step

  

