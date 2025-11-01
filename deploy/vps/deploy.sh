#!/bin/bash

# BrightNest VPS Deployment Script
# This script automates the deployment process to your VPS server

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_NAME="brightnest"
COMPOSE_FILE="../../docker-compose.vps.yml"
ENV_FILE="../../.env"

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
check_prerequisites() {
    print_info "Checking prerequisites..."
    
    if ! command_exists docker; then
        print_error "Docker is not installed. Please install Docker first."
        exit 1
    fi
    
    if ! command_exists docker-compose; then
        print_error "Docker Compose is not installed. Please install Docker Compose first."
        exit 1
    fi
    
    if ! docker info >/dev/null 2>&1; then
        print_error "Docker daemon is not running. Please start Docker first."
        exit 1
    fi
    
    print_success "All prerequisites are met."
}

# Check if .env file exists
check_env_file() {
    if [ ! -f "$ENV_FILE" ]; then
        print_warning "$ENV_FILE not found. Creating from example file..."
        if [ -f "../../env.example" ]; then
            cp ../../env.example "$ENV_FILE"
            print_warning "Please edit $ENV_FILE with your configuration before continuing."
            exit 1
        else
            print_error "env.example not found. Cannot create $ENV_FILE"
            exit 1
        fi
    fi
    print_success "Environment file found at $ENV_FILE"
}

# Stop existing containers
stop_containers() {
    print_info "Stopping existing containers..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" down || true
    print_success "Containers stopped."
}

# Pull latest changes (if using git)
pull_changes() {
    if [ -d ".git" ]; then
        print_info "Pulling latest changes from git..."
        git pull
        print_success "Changes pulled."
    else
        print_warning "Not a git repository. Skipping git pull."
    fi
}

# Build and start containers
build_and_start() {
    print_info "Building and starting containers..."
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --build
    print_success "Containers built and started."
}

# Show container status
show_status() {
    print_info "Container status:"
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" ps
}

# Show logs
show_logs() {
    print_info "Recent logs:"
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" logs --tail=50
}

# Cleanup unused images
cleanup() {
    print_info "Cleaning up unused Docker images..."
    docker image prune -f
    print_success "Cleanup completed."
}

# Main deployment flow
main() {
    print_info "Starting BrightNest VPS deployment..."
    echo ""
    
    check_prerequisites
    check_env_file
    stop_containers
    pull_changes
    build_and_start
    
    echo ""
    print_info "Waiting for services to be healthy (30 seconds)..."
    sleep 30
    
    echo ""
    show_status
    
    echo ""
    print_success "Deployment completed successfully!"
    echo ""
    print_info "Your API Gateway should be available at: http://your-vps-ip:8088"
    print_info "PgAdmin should be available at: http://your-vps-ip:5050"
    print_info "RedisInsight should be available at: http://your-vps-ip:5540"
    echo ""
    print_info "To view logs, run: docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE logs -f"
    print_info "To stop all services, run: docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE down"
    echo ""
    
    read -p "Do you want to see the logs now? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" logs -f
    fi
}

# Run main function
main

