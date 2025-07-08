#!/bin/bash

set -e

if [ ! -d /App/Frierun ]; then
  mkdir -p /App/Frierun
fi

if [ ! -f /App/Frierun/state.json ]; then
  echo "No state.json file found, creating one"
  if [ -f /var/run/docker.sock ]; then 
    cp /App/state.json.default /App/Frierun/state.json
  else
    touch /App/Frierun/state.json
  fi
fi

exec "$@"
