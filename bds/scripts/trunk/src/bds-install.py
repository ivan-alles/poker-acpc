'''
Creates an temporary maven projects to install files from a Maven repository to local machine
Parameters:
%1 Group Id
%2 Artifact Id
%3 Version
Options:
-skip-host-tests do not execute host tests

 
Example:

bds-install.bat ai.examples.packages lib1 1.0-SNAPSHOT

'''

import os
import sys
import shutil
import re
import fileinput

reVersion = re.compile('^([0-9]+).*')

#Dependcy format of dependency plugin:  '   ai.examples.packages:tool1:zip:1.0.2-SNAPSHOT:compile'
reDep = re.compile('^\s*([^:]+):([^:]+):([^:]+):([^:]+)(:.*)$')

skipHostTests = False

paramIdx = 1

for param in sys.argv[1 : ]:
    if param == '-skip-host-tests':
       skipHostTests = True
       continue
    if paramIdx == 1:
       groupId = param
    elif paramIdx == 2:
       artifactId = param
    elif paramIdx == 3:
       version = param
    paramIdx += 1


if not os.path.exists('ai-install-temp'):
    os.mkdir('ai-install-temp')


startDir = os.getcwd()
os.chdir('ai-install-temp')

command = 'mvn archetype:generate -DarchetypeGroupId=ai.bds -DarchetypeArtifactId=install  -DarchetypeVersion=1.0-SNAPSHOT -DgroupId=' + groupId +  ' -DartifactId=' + artifactId + ' -Dversion=' + version + ' -DinteractiveMode=false'

os.system(command)
os.chdir(artifactId)
os.system('mvn compile')
os.system('mvn dependency:tree -DoutputFile=__project-dependencies.tmp -Dtokens=whitespace')

deps = []

for line in fileinput.input('__project-dependencies.tmp'):
    deps.append(line.replace('\n','').replace('\r',''))
fileinput.close()

os.chdir(startDir)

shutil.rmtree('ai-install-temp', True)

aiHome = os.environ['AI_HOME']


if skipHostTests:
    sys.exit(0)


del deps[0] # Remove the install project.
deps.sort()


# deps contains now a list of all dependencies sorted by depth in reversed order.
# The farthest dependencies are at the beginning, the artefact being installed at the end.
for dep in deps:
    print('Prepare host-testing: ' + dep)
    m = reDep.match(dep)
    depGroupId = m.group(1)
    depArtifactId = m.group(2)
    depVersion = m.group(4)
    m = reVersion.match(depVersion)
    if m:
        verMaj = m.group(1)
        os.system('bds-host-test.py ' + depGroupId + ' ' + depArtifactId + ' ' + verMaj)
    else:
        print('Unknown version format for artifact: ' + dep)

