'''
Generate a time-based build number.
The build-number is the number of hours since midnight 01.11.2010, mod-divided by 65536 
(because of the Microsoft limitation of build number range).
It wraps about every 7 years and allows to assign unique build numbers
if builds are done with 1 hour time difference.
'''

import os
import sys
import time

startSt = time.strptime("1 Nov 2010", "%d %b %Y") 
startSec = time.mktime(startSt) 
curSec = time.time()
diffSec = curSec - startSec
#print(diffSec)
buildNum = int(diffSec/3600)
buildNum = buildNum % 65536
print(buildNum)






