#!/usr/bin/env bash
# Task 4: installs the chart, or upgrades the release if it already exists.
# An optional argument sets the Strangler Fig percentage; the proxy pods roll
# automatically because the ConfigMap checksum changes.
#
#   bash deploy/helm-install.sh        # values.yaml as committed (100%)
#   bash deploy/helm-install.sh 50     # move half of /api/movies back to the monolith
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
require helm kubectl

ARGS=(upgrade --install "$RELEASE" "$ROOT/src/kubernetes/helm" --namespace "$NAMESPACE" --create-namespace)
if [ $# -ge 1 ]; then
  ARGS+=(--reuse-values --set "config.moviesMigrationPercent=$1")
else
  # Explicit: without it an upgrade may carry over values set by an earlier
  # run (e.g. the 50% above) instead of returning to values.yaml.
  ARGS+=(--reset-values)
fi

if command -v minikube >/dev/null 2>&1; then
  step "Enabling the minikube ingress addon"
  minikube addons enable ingress
fi

step "helm ${ARGS[*]}"
helm "${ARGS[@]}"

kubectl -n "$NAMESPACE" rollout status deployment/proxy-service --timeout=300s
wait_for_pods
