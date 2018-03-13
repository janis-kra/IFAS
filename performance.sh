#!/bin/bash

#docker-compose up --no-start --build event-generator
#docker-compose up --no-start --build evt-es-bridge


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
> perf-test/performance-$1-$3-$4-$5-$2.txt

echo "running performance test with $1 events, client-side buffersize $2, server-side buffersize $3, live buffer size $4, read batch size $5"
echo "writing performance test results to performance-$1-$3-$4-$5-$2.txt"

cat perf-test/performance-$1-$3-$4-$5-$2.txt

