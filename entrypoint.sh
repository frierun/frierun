#!/bin/bash

set -e

if [ ! -f /App/Frierun/state.json ]; then
  echo "No state.json file found, creating one"
  cp /App/state.json.default /App/Frierun/state.json
fi

exec "$@"
