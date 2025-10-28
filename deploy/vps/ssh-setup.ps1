# SSH Key Setup Script for Windows
# This script helps you set up SSH keys for VPS deployment

param(
    [Parameter(Mandatory=$true)]
    [string]$VpsHost,
    
    [Parameter(Mandatory=$true)]
    [string]$VpsUser,
    
    [string]$KeyPath = "$env:USERPROFILE\.ssh\id_rsa"
)

$ErrorActionPreference = "Stop"

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check if SSH key exists
function Test-SshKey {
    if (Test-Path $KeyPath) {
        Write-Warning "SSH key already exists at: $KeyPath"
        $overwrite = Read-Host "Do you want to create a new key? (y/n)"
        if ($overwrite -ne 'y' -and $overwrite -ne 'Y') {
            return $true
        }
    }
    return $false
}

# Generate SSH key
function New-SshKey {
    Write-Info "Generating new SSH key..."
    
    # Create .ssh directory if it doesn't exist
    $sshDir = Split-Path $KeyPath
    if (-not (Test-Path $sshDir)) {
        New-Item -ItemType Directory -Path $sshDir -Force | Out-Null
    }
    
    # Generate key
    ssh-keygen -t rsa -b 4096 -f $KeyPath -N '""' -C "brightnest-deployment"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "SSH key generated successfully"
        return $true
    } else {
        Write-Error "Failed to generate SSH key"
        return $false
    }
}

# Copy public key to VPS
function Copy-SshKey {
    Write-Info "Copying SSH key to VPS..."
    Write-Info "You will be prompted for your VPS password"
    
    $publicKeyPath = "$KeyPath.pub"
    
    if (-not (Test-Path $publicKeyPath)) {
        Write-Error "Public key not found at: $publicKeyPath"
        return $false
    }
    
    # Read public key
    $publicKey = Get-Content $publicKeyPath -Raw
    
    # Copy to VPS
    $sshCopyCmd = @"
mkdir -p ~/.ssh && \
chmod 700 ~/.ssh && \
echo '$publicKey' >> ~/.ssh/authorized_keys && \
chmod 600 ~/.ssh/authorized_keys && \
echo 'SSH key added successfully'
"@
    
    ssh "$VpsUser@$VpsHost" $sshCopyCmd
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "SSH key copied to VPS successfully"
        return $true
    } else {
        Write-Error "Failed to copy SSH key to VPS"
        return $false
    }
}

# Test SSH connection
function Test-SshConnection {
    Write-Info "Testing SSH connection with key..."
    
    ssh -i $KeyPath -o ConnectTimeout=10 "$VpsUser@$VpsHost" "echo 'SSH key authentication successful'"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "SSH key authentication works!"
        return $true
    } else {
        Write-Error "SSH key authentication failed"
        return $false
    }
}

# Main
function Main {
    Write-Info "SSH Key Setup for BrightNest VPS Deployment"
    Write-Info "=============================================="
    Write-Host ""
    Write-Info "VPS: $VpsUser@$VpsHost"
    Write-Info "Key path: $KeyPath"
    Write-Host ""
    
    # Check if key exists
    $keyExists = Test-SshKey
    
    # Generate new key if needed
    if (-not $keyExists) {
        if (-not (New-SshKey)) {
            exit 1
        }
        Write-Host ""
    }
    
    # Copy key to VPS
    if (-not (Copy-SshKey)) {
        Write-Host ""
        Write-Warning "Manual setup required:"
        Write-Host "1. Copy the content of: $KeyPath.pub"
        Write-Host "2. Connect to your VPS: ssh $VpsUser@$VpsHost"
        Write-Host "3. Run these commands:"
        Write-Host "   mkdir -p ~/.ssh"
        Write-Host "   nano ~/.ssh/authorized_keys"
        Write-Host "4. Paste the key content and save"
        Write-Host "5. Run: chmod 600 ~/.ssh/authorized_keys"
        exit 1
    }
    
    Write-Host ""
    
    # Test connection
    if (-not (Test-SshConnection)) {
        Write-Error "SSH key setup failed"
        exit 1
    }
    
    Write-Host ""
    Write-Success "SSH key setup completed successfully!"
    Write-Host ""
    Write-Info "You can now deploy without entering a password:"
    Write-Info ".\deploy-from-windows.ps1 -VpsHost $VpsHost -VpsUser $VpsUser"
}

# Check if ssh-keygen is available
if (-not (Get-Command ssh-keygen -ErrorAction SilentlyContinue)) {
    Write-Error "ssh-keygen not found. Please install OpenSSH client."
    Write-Info "Install using: Add-WindowsCapability -Online -Name OpenSSH.Client~~~~0.0.1.0"
    exit 1
}

# Run main
try {
    Main
} catch {
    Write-Error "An error occurred: $_"
    exit 1
}

