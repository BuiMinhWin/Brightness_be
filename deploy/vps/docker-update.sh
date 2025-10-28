#!/bin/bash

# Quick update script for BrightNest
# Use this to quickly update specific services without full redeployment

set -e

COMPOSE_FILE="../../docker-compose.vps.yml"
ENV_FILE="../../.env.vps"

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

# Check if service name is provided
if [ -z "$1" ]; then
    print_info "Usage: ./docker-update.sh [service-name|all]"
    print_info "Available services:"
    echo "  - gateway-api"
    echo "  - authentication-api"
    echo "  - measurement-api"
    echo "  - challenge-api"
    echo "  - social-api"
    echo "  - billing-api"
    echo "  - media-api"
    echo "  - chat-service"
    echo "  - ai-ocr-service"
    echo "  - personal-coach-api"
    echo "  - all (update all services)"
    exit 1
fi

SERVICE=$1

if [ "$SERVICE" == "all" ]; then
    print_info "Updating all services..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --build
    print_success "All services updated!"
else
    print_info "Updating $SERVICE..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --build --no-deps "$SERVICE"
    print_success "$SERVICE updated!"
fi

print_info "Viewing logs for $SERVICE..."
docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" logs --tail=50 -f "$SERVICE"

