#!/bin/sh
set -e
if [ -n "$PUBLIC_API_BASE" ]; then
  printf '{ "ApiBaseUrl": "%s" }' "$PUBLIC_API_BASE" > /usr/share/nginx/html/appsettings.json
fi 