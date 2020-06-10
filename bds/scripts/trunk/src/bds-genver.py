'''
Generates version information file.

Currently generates a file for C# only. 
'''

import os
import sys
import re
import subprocess
import datetime
from socket import gethostname

from optparse import OptionParser

parser = OptionParser()
parser.add_option("-p", "--pom-version", dest="pomVersion", help="POM version", default="")
parser.add_option("-c", "--cfg", dest="cfg", help="Build configuration", default="")
parser.add_option("-d", "--descr", dest="descr", help="Description", default="")
parser.add_option("-o", "--output", dest="output", help="Output file")

(options, args) = parser.parse_args()

scriptPath = os.path.abspath(__file__)
scriptDir = os.path.dirname(scriptPath)

reVersion = re.compile('([0-9]+)\.([0-9]+)\.([0-9]+)(-.*)?')

m = reVersion.match(options.pomVersion)

verMajor = m.group(1)
verMinor = m.group(2)
verRevision = m.group(3)
verQualifier = m.group(4)

svnInfo = subprocess.check_output(["python", scriptDir + "/bds-svn-info.py"], stderr=subprocess.STDOUT)
svnInfo = str(svnInfo, encoding='utf8').replace('\n','').replace('\r','')
#print(svnInfo)

buildNum = subprocess.check_output(["python", scriptDir + "/bds-buildnum.py"], stderr=subprocess.STDOUT)
buildNum = str(buildNum, encoding='utf8').replace('\n','').replace('\r','')
#print(buildNum)

buildTime = datetime.datetime.now()

buildInfo = 'POM: ' + options.pomVersion + ', cfg: ' + options.cfg + ', time: ' + buildTime.strftime('%d.%m.%y %H:%M:%S') + ', host: ' + gethostname()
description = options.descr

outDir = os.path.dirname(options.output)
if outDir and not os.path.exists(outDir):
    os.mkdir(outDir)

outFile = open(options.output, 'w')

versionString = verMajor + '.' + verMinor + '.' + buildNum + '.' + verRevision


print('// This file is auto-generated', file=outFile)
print('using System.Reflection;', file=outFile)

print('[assembly: AssemblyVersion("' + versionString + '")]', file=outFile)
print('[assembly: AssemblyFileVersion("' + versionString + '")]', file=outFile)
print('[assembly: AssemblyDescription("' + description + '")]', file=outFile)
print('[assembly: AssemblyInformationalVersion("' + svnInfo + '")]', file=outFile)
print('[assembly: AssemblyConfiguration("' + buildInfo + '")]', file=outFile)





