#!/usr/bin/env bash
# Exposes the ingress on 127.0.0.1 so https://cinemaabyss.example.com works in a
# browser. Keep it running; stop with Ctrl+C.
#
# Requires the hosts entry (as administrator):
#   127.0.0.1 cinemaabyss.example.com   ->  C:\Windows\System32\drivers\etc\hosts
#
#   bash deploy/minikube-tunnel.sh
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"
require minikube

minikube tunnel
