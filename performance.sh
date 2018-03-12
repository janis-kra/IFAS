#!/bin/bash

#docker-compose up --no-start --build event-generator
#docker-compose up --no-start --build evt-es-bridge

docker-compose run -e EXPERIMENT_SIZE=$1 \
  --rm -d event-generator \
  && \
  docker-compose run -e STREAM=performance-$1\
    -e GROUP=performance\
    -e EXPECTED_AMOUNT=$1\
    --rm evt-es-bridge

