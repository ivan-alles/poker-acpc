'''
Prints information about svn repository for the current folder:

<path>:<rev>

where:
path : is a path cut after one dir above the trunk, tags or branches folder, for example lib1/trunk
rev  : is the result of a call to svnversion

 
'''

import os
import sys
import re
import subprocess

reSvnUrl = re.compile('.*<url>(.*)</url>', re.DOTALL)
reSvnUrlCut = re.compile('.*/([^/]+/)(tags|trunk|branches)(.*)')


try:

    svnInfo = subprocess.check_output(["svn", "info", "--xml"], stderr=subprocess.STDOUT)
    svnInfo = str(svnInfo, encoding='utf8')
    #print (svnInfo)

    m = reSvnUrl.match(svnInfo)
    if not m: raise Exception('Cannot find svn url')
    svnUrl  = m.group(1)
    m = reSvnUrlCut.match(svnUrl)
    if m:
        svnUrl = m.group(1) + m.group(2) + m.group(3)


    svnRev = subprocess.check_output(["svnversion"], stderr=subprocess.STDOUT)
    svnRev = str(svnRev, encoding='utf8')

    svnPathString = svnUrl + ':' + svnRev


except:
    svnPathString = 'svn info n/a'


print(svnPathString)







