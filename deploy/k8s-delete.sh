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

# Volumes the namespace left behind. minikube's hostpath provisioner names its
# folders after the claim (cinemaabyss/kafka-data), so a stale folder is picked
# up again by the next install: Kafka then finds the previous cluster id in
# meta.properties and fails with InconsistentClusterIdException.
step "Removing leftover volumes of $NAMESPACE"
for pv in $(kubectl get pv -o jsonpath="{range .items[?(@.spec.claimRef.namespace=='$NAMESPACE')]}{.metadata.name}{'\n'}{end}"); do
  kubectl delete pv "$pv" --ignore-not-found
done
if command -v minikube >/dev/null 2>&1; then
  # MSYS_NO_PATHCONV: Git Bash would otherwise turn the Linux path into a Windows one.
  MSYS_NO_PATHCONV=1 minikube ssh -- sudo rm -rf "/tmp/hostpath-provisioner/$NAMESPACE"
fi
