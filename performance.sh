#!/bin/bash

#docker-compose up --no-start --build event-generator
#docker-compose up --no-start --build evt-es-bridge

if [ -z ${1+x} ]; then EXP_SIZE=$1; else EXP_SIZE=100; fi
if [ -z ${2+x} ]; then CLIENT_BUFF_SIZE=$2; else CLIENT_BUFF_SIZE=100; fi
if [ -z ${3+x} ]; then BUFF_SIZE=$2; else BUFF_SIZE=500; fi
if [ -z ${4+x} ]; then LIVE_BUFF_SIZE=$3; else LIVE_BUFF_SIZE=500; fi
if [ -z ${5+x} ]; then READ_BATCH=$5; else READ_BATCH=20; fi

touch perf-test-$7/performance-$6.csv

docker-compose run -e EXPERIMENT_SIZE=$1 \
    -e BUFFER_SIZE=$3 \
    -e LIVE_BUFFER_SIZE=$4 \
    -e READ_BATCH=$5 \
    -d --rm event-generator

docker-compose run -e STREAM=performance-$1\
    -e GROUP=performance \
    -e EXPECTED_AMOUNT=$1 \
    -e BUFFER_SIZE=$2 \
    --name evt-es-bridge-$1 \
    --rm evt-es-bridge \
> perf-test-$7/performance-$6.csv

echo "running performance test with $1 events, client-side buffersize $2, server-side buffersize $3, live buffer size $4, read batch size $5"
cat perf-test-$7/performance-$6.csv

