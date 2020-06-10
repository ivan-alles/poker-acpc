'''
Generates protobuf-net cs file

'''

import os
import sys
import shutil
import subprocess

protogenDir='..\\..\\..\\..\\target\\dist\\build\\'
src='Protocol.proto'
dst='Protocol.cs'


generate = 0

if not os.path.exists('Remote\\'+dst):
  generate = 1

if not generate:
  srcMtime = os.path.getmtime('Remote\\'+src)
  dstMtime = os.path.getmtime('Remote\\'+dst)
  generate = ( srcMtime > dstMtime )


if not generate:
   print("Source is older than target - nothing to do.")
   sys.exit()

print("Generate target...")

# We have to copy proto file to the folder of protogen, 
# because it doesn't understand paths.

shutil.copy('remote\\'+src, protogenDir)

startDir = os.getcwd()
os.chdir(protogenDir)

retcode = subprocess.call('protogen.exe -i:' + src + ' -o:' + dst, shell=True, stderr=subprocess.STDOUT)
if retcode != 0:
    print("Error: protogen.exe returned ", retcode, file=sys.stderr)
    sys.exit()

os.chdir(startDir)

shutil.copy(protogenDir+'\\'+dst, 'remote\\')


