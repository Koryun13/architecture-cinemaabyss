#!/usr/bin/env bash
# Runs the Postman "kubernetes" environment from a pod inside the cluster.
#
# The requests still go through the ingress: cinemaabyss.example.com is mapped
# to the ingress controller with hostAliases. This needs neither a hosts-file
# entry nor `minikube tunnel`, which is convenient for a quick check.
#
#   bash deploy/postman-tests-in-cluster.sh
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
require docker kubectl minikube

IMAGE="cinemaabyss-api-tests:latest"

step "Building the test image and loading it into minikube"
docker build -q -t "$IMAGE" "$ROOT/tests/postman" >/dev/null
minikube image load "$IMAGE"

INGRESS_IP="$(kubectl -n ingress-nginx get svc ingress-nginx-controller -o jsonpath='{.spec.clusterIP}')"
OVERRIDES="{\"spec\":{\"hostAliases\":[{\"ip\":\"$INGRESS_IP\",\"hostnames\":[\"cinemaabyss.example.com\"]}]}}"

step "Running the tests against http://cinemaabyss.example.com (ingress $INGRESS_IP)"
kubectl -n "$NAMESPACE" delete pod api-tests --ignore-not-found >/dev/null
kubectl -n "$NAMESPACE" run api-tests --image="$IMAGE" --image-pull-policy=Never --restart=Never \
  --overrides="$OVERRIDES" -- --environment kubernetes >/dev/null

until kubectl -n "$NAMESPACE" get pod api-tests -o jsonpath='{.status.phase}' | grep -qE 'Succeeded|Failed'; do
  sleep 2
done

kubectl -n "$NAMESPACE" logs api-tests
PHASE="$(kubectl -n "$NAMESPACE" get pod api-tests -o jsonpath='{.status.phase}')"
kubectl -n "$NAMESPACE" delete pod api-tests >/dev/null
[ "$PHASE" = "Succeeded" ]
