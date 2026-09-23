#!/usr/bin/env bash
# Runs the Postman collection with Newman.
#
#   bash deploy/postman-tests.sh local        # services on localhost ports (docker compose)
#   bash deploy/postman-tests.sh docker       # inside the compose network, exactly as CI does
#   bash deploy/postman-tests.sh kubernetes   # through the ingress (hosts entry + minikube tunnel)
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

ENVIRONMENT="${1:-local}"
TESTS="$ROOT/tests/postman"

case "$ENVIRONMENT" in
  docker)
    require docker
    step "Building the test image and running it in cinemaabyss-network (as in api-tests.yml)"
    docker build -q -t cinemaabyss-api-tests "$TESTS" >/dev/null
    docker run --rm --network=cinemaabyss-network cinemaabyss-api-tests
    ;;
  local|kubernetes)
    require npm
    cd "$TESTS"
    if [ ! -d node_modules ]; then
      step "Installing Newman"
      npm ci --no-audit --no-fund
    fi
    npm run "test:$ENVIRONMENT"
    ;;
  *)
    echo "usage: postman-tests.sh local|docker|kubernetes" >&2
    exit 2
    ;;
esac
