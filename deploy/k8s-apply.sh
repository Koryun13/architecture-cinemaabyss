#!/usr/bin/env bash
# Task 3: deploys the raw manifests from src/kubernetes in the order given in
# Project_template.md, then the ingress.
#
#   bash deploy/k8s-apply.sh
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
require kubectl

K8S="$ROOT/src/kubernetes"

if command -v minikube >/dev/null 2>&1; then
  step "Enabling the minikube ingress addon"
  minikube addons enable ingress
fi

step "Namespace, config and secrets"
kubectl apply -f "$K8S/namespace.yaml"
kubectl apply -f "$K8S/configmap.yaml"
kubectl apply -f "$K8S/secret.yaml"
kubectl apply -f "$K8S/dockerconfigsecret.yaml"
kubectl apply -f "$K8S/postgres-init-configmap.yaml"

step "PostgreSQL and Kafka"
kubectl apply -f "$K8S/postgres.yaml"
kubectl apply -f "$K8S/kafka/kafka.yaml"

step "Monolith and microservices"
kubectl apply -f "$K8S/monolith.yaml"
kubectl apply -f "$K8S/movies-service.yaml"
kubectl apply -f "$K8S/events-service.yaml"
kubectl apply -f "$K8S/proxy-service.yaml"

step "Ingress"
kubectl -n ingress-nginx wait --for=condition=Ready pod -l app.kubernetes.io/component=controller --timeout=300s
kubectl apply -f "$K8S/ingress.yaml"

wait_for_pods
