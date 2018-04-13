#!/bin/bash
docker-compose -f docker-compose.yml -f client/docker-compose.yml up -d kibana elasticsearch eventstore evt-es-bridge app db web
