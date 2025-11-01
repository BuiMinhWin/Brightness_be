#!/bin/bash

# VPS Initial Setup Script for BrightNest
# This script prepares a fresh VPS for hosting the BrightNest application

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

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

# Update system
update_system() {
    print_info "Updating system packages..."
    sudo apt-get update
    sudo apt-get upgrade -y
    print_success "System updated."
}

# Install Docker
install_docker() {
    if command -v docker >/dev/null 2>&1; then
        print_warning "Docker is already installed."
        docker --version
        return
    fi
    
    print_info "Installing Docker..."
    
    # Install prerequisites
    sudo apt-get install -y \
        ca-certificates \
        curl \
        gnupg \
        lsb-release
    
    # Add Docker's official GPG key
    sudo mkdir -p /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
    
    # Set up Docker repository
    echo \
      "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
      $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
    
    # Install Docker Engine
    sudo apt-get update
    sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
    
    # Add current user to docker group
    sudo usermod -aG docker $USER
    
    print_success "Docker installed successfully."
    print_warning "Please log out and log back in for group changes to take effect."
}

# Install Docker Compose standalone (optional, for older systems)
install_docker_compose() {
    if command -v docker-compose >/dev/null 2>&1; then
        print_warning "Docker Compose is already installed."
        docker-compose --version
        return
    fi
    
    print_info "Installing Docker Compose..."
    
    DOCKER_COMPOSE_VERSION="v2.24.0"
    sudo curl -L "https://github.com/docker/compose/releases/download/${DOCKER_COMPOSE_VERSION}/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
    
    print_success "Docker Compose installed successfully."
}

# Install Git
install_git() {
    if command -v git >/dev/null 2>&1; then
        print_warning "Git is already installed."
        git --version
        return
    fi
    
    print_info "Installing Git..."
    sudo apt-get install -y git
    print_success "Git installed successfully."
}

# Configure firewall
configure_firewall() {
    print_info "Configuring firewall..."
    
    # Install UFW if not installed
    if ! command -v ufw >/dev/null 2>&1; then
        sudo apt-get install -y ufw
    fi
    
    # Configure firewall rules
    sudo ufw default deny incoming
    sudo ufw default allow outgoing
    sudo ufw allow ssh
    sudo ufw allow 8088/tcp  # API Gateway
    sudo ufw allow 5050/tcp  # PgAdmin (optional, remove if not needed)
    sudo ufw allow 5540/tcp  # RedisInsight (optional, remove if not needed)
    sudo ufw allow 80/tcp    # HTTP
    sudo ufw allow 443/tcp   # HTTPS
    
    print_warning "Firewall rules configured but not enabled yet."
    print_warning "To enable firewall, run: sudo ufw enable"
    print_warning "Make sure SSH is working before enabling!"
}

# Create project directory
create_project_directory() {
    PROJECT_DIR="/opt/brightnest"
    
    print_info "Creating project directory..."
    sudo mkdir -p "$PROJECT_DIR"
    sudo chown $USER:$USER "$PROJECT_DIR"
    print_success "Project directory created at $PROJECT_DIR"
}

# Install monitoring tools (optional)
install_monitoring() {
    print_info "Installing monitoring tools..."
    sudo apt-get install -y htop iotop nethogs
    print_success "Monitoring tools installed."
}

# Configure swap (if needed)
configure_swap() {
    if [ -f /swapfile ]; then
        print_warning "Swap file already exists."
        return
    fi
    
    print_info "Configuring swap file (2GB)..."
    sudo fallocate -l 2G /swapfile
    sudo chmod 600 /swapfile
    sudo mkswap /swapfile
    sudo swapon /swapfile
    echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
    print_success "Swap configured."
}

# Setup log rotation for Docker
setup_log_rotation() {
    print_info "Setting up Docker log rotation..."
    
    sudo mkdir -p /etc/docker
    cat <<EOF | sudo tee /etc/docker/daemon.json
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
EOF
    
    print_success "Docker log rotation configured."
    print_warning "Docker needs to be restarted for changes to take effect."
}

# Install Nginx (optional, for reverse proxy)
install_nginx() {
    if command -v nginx >/dev/null 2>&1; then
        print_warning "Nginx is already installed."
        return
    fi
    
    read -p "Do you want to install Nginx as reverse proxy? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_info "Installing Nginx..."
        sudo apt-get install -y nginx
        print_success "Nginx installed."
        print_info "You can configure Nginx as a reverse proxy later."
    fi
}

# Main setup
main() {
    print_info "Starting VPS setup for BrightNest..."
    echo ""
    
    update_system
    echo ""
    
    install_docker
    echo ""
    
    install_docker_compose
    echo ""
    
    install_git
    echo ""
    
    configure_firewall
    echo ""
    
    create_project_directory
    echo ""
    
    install_monitoring
    echo ""
    
    configure_swap
    echo ""
    
    setup_log_rotation
    echo ""
    
    install_nginx
    echo ""
    
    print_success "VPS setup completed!"
    echo ""
    print_info "Next steps:"
    echo "1. Log out and log back in to apply Docker group changes"
    echo "2. Clone your repository to /opt/brightnest"
    echo "3. Copy .env.example to .env and configure it"
    echo "4. Run the deployment script: ./deploy/vps/deploy.sh"
    echo ""
    print_warning "Security recommendations:"
    echo "- Change SSH port from default 22"
    echo "- Disable root login"
    echo "- Set up SSH key authentication only"
    echo "- Enable firewall: sudo ufw enable"
    echo "- Set up SSL certificates for HTTPS"
    echo "- Configure Nginx as reverse proxy"
    echo "- Set up automatic security updates"
}

# Check if running as root
if [ "$EUID" -eq 0 ]; then 
    print_error "Please do not run this script as root. Run as a regular user with sudo privileges."
    exit 1
fi

# Run main function
main

