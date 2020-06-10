'''
Sets some predefined properties in a POM file.

1st scan of the file calculates the values of the properties.
2nd scan updates the properties and writes a temporary file.
Then the original file is replaced with the temporary one.

Current limitations:

No XML parsing is done, the file is interpreted as a text file 
using regular expressions. Therefore it is highly recommended to write one XML tag
per line.

For project.version there are other properties (e.g. dependency version) that
looks the same for the simple text parser. As a workaround, the 1st <version> tag is 
taken, the others are ignored.

'''

import sys
import re
import fileinput
import shutil

reVersion = re.compile('.*<version>([0-9]+)\.([0-9]+)\.([0-9]+)(-.*)?</version>')

props = {}


for line in fileinput.input('pom.xml'): 
    if not 'ai.ver.major' in props: # Ignore 2nd, etc. <version> tags
        m = reVersion.match(line)
        if m:
             props['ai.ver.major'] = m.group(1)
             props['ai.ver.minor'] = m.group(2)
             props['ai.ver.revision'] = m.group(3)
             if m.group(4):
                 props['ai.ver.qualifier'] = m.group(4)[1:] # Cut off the - separator
             else:
                 props['ai.ver.qualifier'] = ''

             #print(props['ai.ver.major'], props['ai.ver.minor'], props['ai.ver.revision'], props['ai.ver.qualifier'])

fileinput.close() 

outFile = open('pom.xml.tmp', 'w')


# Does a list of files, and writes redirects STDOUT to the file in question
for line in fileinput.input('pom.xml'): 
    
    for prop, value in props.items():
        # Try <prop>value</prop>
        reProp = re.compile('^(.*<' + prop + '\s*>)([^<]*)(</' + prop + '\s*>.*)$')
        m = reProp.match(line)
        if m:
           line = m.group(1) + value + m.group(3) + '\n'
           break
        else:
           #Try </prop>
           reProp = re.compile('(^.*)<' + prop + '\s*/>(.*)$')
           m = reProp.match(line)
           if m:
               line = m.group(1) + "<" + prop + ">" + value + "</" + prop + ">" + m.group(2) + "\n"
               break
    print(line, end = '', file = outFile)

fileinput.close() 
outFile.close()

shutil.move('pom.xml.tmp', 'pom.xml')




