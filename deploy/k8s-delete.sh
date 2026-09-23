#!/usr/bin/env bash
# Removes everything from the cinemaabyss namespace (raw manifests or Helm alike),
# as in the "Удаляем все" section of Project_template.md.
#
#   bash deploy/k8s-delete.sh
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
require kubectl

if command -v helm >/dev/null 2>&1 && helm status "$RELEASE" -n "$NAMESPACE" >/dev/null 2>&1; then
  step "Uninstalling Helm release $RELEASE"
  helm uninstall "$RELEASE" -n "$NAMESPACE"
fi

step "Deleting namespace $NAMESPACE"
kubectl delete all --all -n "$NAMESPACE" --ignore-not-found
kubectl delete namespace "$NAMESPACE" --ignore-not-found
