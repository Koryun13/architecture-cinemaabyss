#!/usr/bin/env bash
# Stops the Docker Compose stack and drops its volumes (PostgreSQL data, Kafka topics).
#
#   bash deploy/compose-down.sh
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
require docker

cd "$ROOT"
docker compose down -v
