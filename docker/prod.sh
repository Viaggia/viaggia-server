#!/bin/bash

# Production Environment
echo "🚀 Starting Viaggia API in Production mode..."

export ENVIRONMENT=production
export ASPNETCORE_ENVIRONMENT=Production
export API_PORT=80
export DOCKERFILE=Dockerfile
# No volume mounts for production
unset VOLUME_MOUNT
unset BIN_VOLUME  
unset OBJ_VOLUME

docker compose up --build -d

echo "✅ Production server running at: http://localhost"
