#!/usr/bin/env bash
# Sends N requests to /api/movies through the proxy and counts which backend
# answered (X-Upstream-Service header), making the Strangler Fig split visible.
#
#   bash deploy/traffic-split.sh                              # compose proxy, 200 requests
#   bash deploy/traffic-split.sh http://cinemaabyss.example.com 100
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
require curl

BASE_URL="${1:-http://localhost:8000}"
COUNT="${2:-200}"

step "GET $BASE_URL/api/movies x $COUNT"
for _ in $(seq 1 "$COUNT"); do
  curl -s -o /dev/null -D - "$BASE_URL/api/movies" | tr -d '\r' | grep -i '^x-upstream-service:' | cut -d' ' -f2
done | sort | uniq -c
