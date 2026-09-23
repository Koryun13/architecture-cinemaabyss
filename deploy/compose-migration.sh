#!/usr/bin/env bash
# Strangler Fig in Docker Compose: recreates only the proxy with another
# MOVIES_MIGRATION_PERCENT, without editing docker-compose.yml.
#
#   bash deploy/compose-migration.sh 75
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
require docker

PERCENT="${1:?usage: compose-migration.sh <percent 0..100>}"

cd "$ROOT"
step "Recreating proxy-service with MOVIES_MIGRATION_PERCENT=$PERCENT"
MOVIES_MIGRATION_PERCENT="$PERCENT" docker compose up -d --no-deps proxy-service

until curl -fsS -o /dev/null http://localhost:8000/health; do sleep 1; done
bash "$ROOT/deploy/traffic-split.sh" http://localhost:8000 100
