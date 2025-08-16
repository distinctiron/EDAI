#!/bin/bash

echo "Starting container with postgres db for EDAI..."
docker run --name edai-postgres-local \
-e POSTGRES_USER=edai_user \
-e POSTGRES_PASSWORD=edai_password \
-e POSTGRES_DB=edai_db \
-p 5431:5432 \
-d postgres