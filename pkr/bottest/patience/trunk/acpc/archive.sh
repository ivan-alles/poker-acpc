#!/bin/bash
# Archive necessary files to send to ACPC server (except the strategy folder, it is too big)

tar -cvjf patience.tar.bz2 acpc-scripts submission \
target/dist/bin/*.dll target/dist/bin/ai.pkr.acpc.server-adapter.exe \
target/dist/data target/dist/pkr \
creation-params-1.xml  \
creation-params-2.xml  \
run-patience.sh










