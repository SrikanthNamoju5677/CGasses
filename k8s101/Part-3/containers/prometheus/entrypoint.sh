#!/bin/sh
set -e
#
function dig() {
    nslookup host.docker.internal|grep Address:|grep -v 127.0.0|cut -d: -f2
}
HOST_IP=$(dig; while [ $? -ne 0 ]; do dig; done)

echo "$HOST_IP host.docker.internal" >> /etc/hosts

exec /bin/prometheus "$@"