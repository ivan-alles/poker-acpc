'''
Creates a .NET BDS package based on ai.bsd.package-net artifact.
Parameters:
%1 Group Id
%2 Artifact Id
 
Example:

aibds-package-net.py ai.mygroup mypackage
'''

import os
import sys

groupId = sys.argv[1]
artifactId = sys.argv[2]

command = 'mvn archetype:generate -DarchetypeGroupId=ai.bds -DarchetypeArtifactId=package-net  -DarchetypeVersion=1.0-SNAPSHOT -DgroupId=' + groupId + ' -DartifactId='+artifactId + ' -DinteractiveMode=false'
os.system(command)

os.rename(artifactId + '/package.sln', artifactId + '/' + artifactId + '.sln')


os.mkdir(artifactId + '/src/main/net')



