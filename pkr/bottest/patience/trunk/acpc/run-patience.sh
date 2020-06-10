#!/bin/bash
# Runs patience with acpc adapter
# Parameters:
# $1 hostname
# $2 port
# $3 file with creation parameters (assuming we are in $BOTTEST_ROOT)

echo Run Patience for host: $1 port: $2

#SCRIPT=`readlink -f $0`
#SCRIPTDIR=`dirname $SCRIPT`
SCRIPTDIR=$(pwd)
#echo run-patience.sh is started from: $SCRIPTDIR

export BOTTEST_ROOT=$SCRIPTDIR/
export BOTTEST_BIN=$SCRIPTDIR/target/dist/bin/
export BOTTEST_BOTBIN=$SCRIPTDIR/target/dist/pkr/bots/patience/1.0/

# We have to change directory to bin directory, otherwise there are problems with
# creating types by assembly name.


pushd $BOTTEST_BIN

mono ai.pkr.acpc.server-adapter.exe -v --server-addr:$1:$2 --bot-class:ai.pkr.bots.patience.Patience\;$BOTTEST_BOTBIN/ai.pkr.bots.patience.dll --creation-params:$BOTTEST_ROOT/$3 -d:bds.VarDir=$SCRIPTDIR/target/dist/var/ -d:bds.TraceDir=$SCRIPTDIR/target/dist/var/trace

popd





