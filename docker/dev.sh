#!/bin/bash

# Development Environment
echo "🚀 Starting Viaggia API in Development mode..."

export ENVIRONMENT=development
export ASPNETCORE_ENVIRONMENT=Development
export API_PORT=5001
export DOCKERFILE=Dockerfile.dev
export VOLUME_MOUNT=../:/app
export BIN_VOLUME=/app/bin
export OBJ_VOLUME=/app/obj

docker compose up --build

echo "✅ Development server running at: http://localhost:5001"
echo "📚 Swagger UI: http://localhost:5001/swagger"
