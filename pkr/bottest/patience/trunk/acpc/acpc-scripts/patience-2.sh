#!/bin/bash

echo Patience script is started by the ACPC server with host: $1 port: $2

cd /mnt/exchange/acpc/

./run-patience.sh $1 $2 creation-params-2.xml



