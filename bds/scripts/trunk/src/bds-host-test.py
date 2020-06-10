'''
Runs a host-test for a specified artifact installed on this host.
Parameters:
%1 group id
%2 artifact Id
%3 major version
 
Example:

bds-host-test.bat ai.examples.packages lib1 1
'''

import os
import sys

groupId = sys.argv[1]
artifactId = sys.argv[2]
verMaj = sys.argv[3]

testPom=os.environ['AI_HOME']+'/test/' + groupId + '.' + artifactId + '.' + verMaj + '.host-test.xml'

if not os.path.exists(testPom):
  print('Test ' + testPom + ' does not exist, nothing to do.')  
  sys.exit()

reportDir = os.environ['AI_HOME']+'/var/host-test-reports'

if not os.path.exists(reportDir):
    os.mkdir(reportDir)


os.system('mvn test  -f ' + testPom + ' -Dai.test.reportDir=' + reportDir)

