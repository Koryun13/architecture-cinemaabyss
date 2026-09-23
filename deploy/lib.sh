#!/usr/bin/env bash
# Shared helpers for the deploy/*.sh scripts. Sourced, not executed.

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
NAMESPACE="cinemaabyss"
RELEASE="cinemaabyss"

# Fails with a readable message instead of "command not found" halfway through.
require() {
  for tool in "$@"; do
    if ! command -v "$tool" >/dev/null 2>&1; then
      echo "error: '$tool' is not on PATH." >&2
      echo "       Install it, then restart the IDE/terminal so it picks up the new PATH." >&2
      exit 1
    fi
  done
}

step() {
  printf '\n\033[1m==> %s\033[0m\n' "$*"
}

# Waits until every Deployment and StatefulSet in the namespace has rolled out
# (Kafka and PostgreSQL take a while). Rollout status is used rather than
# `kubectl wait pod --all`, which would also wait on the old pods an upgrade replaces.
wait_for_pods() {
  step "Waiting for workloads in namespace $NAMESPACE"
  local workload
  for workload in $(kubectl -n "$NAMESPACE" get deployment,statefulset -o name); do
    kubectl -n "$NAMESPACE" rollout status "$workload" --timeout=600s
  done
  kubectl -n "$NAMESPACE" get pods
}
