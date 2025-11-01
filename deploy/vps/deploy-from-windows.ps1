# BrightNest VPS Deployment Script for Windows
# This script deploys from Windows to your VPS server

param(
    [Parameter(Mandatory=$true)]
    [string]$VpsHost,
    
    [Parameter(Mandatory=$true)]
    [string]$VpsUser,
    
    [string]$VpsPath = "/opt/brightnest",
    
    [string]$SshKeyPath = "$env:USERPROFILE\.ssh\id_rsa",
    
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

# Colors for output
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

# Check if SSH is available
function Test-SshConnection {
    Write-Info "Testing SSH connection to $VpsUser@$VpsHost..."
    
    try {
        if (Test-Path $SshKeyPath) {
            ssh -i $SshKeyPath -o ConnectTimeout=10 "$VpsUser@$VpsHost" "echo 'Connection successful'" 2>$null
        } else {
            ssh -o ConnectTimeout=10 "$VpsUser@$VpsHost" "echo 'Connection successful'" 2>$null
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "SSH connection successful"
            return $true
        }
    } catch {
        Write-Error "SSH connection failed: $_"
        return $false
    }
    
    Write-Error "Cannot connect to VPS. Please check your credentials."
    return $false
}

# Ensure VPS directory exists
function Ensure-VpsDirectory {
    Write-Info "Ensuring VPS directory exists..."
    
    $ensureDirCmd = "mkdir -p $VpsPath && echo 'Directory ready'"
    
    if (Test-Path $SshKeyPath) {
        ssh -i $SshKeyPath "$VpsUser@$VpsHost" $ensureDirCmd
    } else {
        ssh "$VpsUser@$VpsHost" $ensureDirCmd
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "VPS directory is ready"
        return $true
    } else {
        Write-Error "Failed to create VPS directory"
        return $false
    }
}

# Sync files to VPS using rsync or scp
function Sync-Files {
    Write-Info "Syncing files to VPS..."
    
    # Get the Brightness_be root directory (go up 2 levels from deploy/vps)
    $currentPath = (Get-Location).Path
    if ($currentPath -like "*\deploy\vps") {
        $sourcePath = Split-Path (Split-Path $currentPath -Parent) -Parent
    } elseif ($currentPath -like "*\Brightness_be") {
        $sourcePath = $currentPath
    } else {
        # Assume we're in the Brightness_be directory
        $sourcePath = $currentPath
    }
    
    Write-Info "Source directory: $sourcePath"
    
    # Check if rsync is available (Git Bash or WSL)
    $rsyncAvailable = Get-Command rsync -ErrorAction SilentlyContinue
    
    if ($rsyncAvailable) {
        Write-Info "Using rsync for file transfer..."
        
        # Convert Windows path to Unix-style path for rsync
        $unixPath = $sourcePath -replace '\\', '/' -replace '^([A-Z]):', '/mnt/$1'.ToLower()
        
        if (Test-Path $SshKeyPath) {
            rsync -avz --delete `
                --exclude='.git' `
                --exclude='bin' `
                --exclude='obj' `
                --exclude='node_modules' `
                --exclude='.vs' `
                --exclude='.vscode' `
                --exclude='*.user' `
                --exclude='*.suo' `
                --exclude='.env.local' `
                --exclude='.env.development' `
                --exclude='TestResults' `
                --exclude='*.log' `
                -e "ssh -i $SshKeyPath" `
                "$sourcePath/" "$VpsUser@$VpsHost`:$VpsPath/Brightness_be/"
        } else {
            rsync -avz --delete `
                --exclude='.git' `
                --exclude='bin' `
                --exclude='obj' `
                --exclude='node_modules' `
                --exclude='.vs' `
                --exclude='.vscode' `
                --exclude='*.user' `
                --exclude='*.suo' `
                --exclude='.env.local' `
                --exclude='.env.development' `
                --exclude='TestResults' `
                --exclude='*.log' `
                "$sourcePath/" "$VpsUser@$VpsHost`:$VpsPath/Brightness_be/"
        }
    } else {
        Write-Warning "rsync not found. Using scp (slower)..."
        
        # Create a temporary archive
        $tempZip = "$env:TEMP\brightnest-deploy.zip"
        Write-Info "Creating temporary archive..."
        
        # Remove old archive if exists
        if (Test-Path $tempZip) {
            Remove-Item $tempZip -Force
        }
        
        # Define patterns to exclude
        $excludePatterns = @(
            "bin",
            "obj",
            "node_modules",
            ".git",
            ".vs",
            ".vscode",
            "*.user",
            "*.suo",
            ".env.local",
            ".env.development",
            "TestResults",
            "*.log"
        )
        
        Write-Info "Collecting files (excluding build artifacts)..."
        
        # Get all files except excluded ones
        Push-Location $sourcePath
        
        # Create a list of files to include
        $filesToInclude = Get-ChildItem -Path "." -Recurse -File | Where-Object {
            $file = $_
            $shouldExclude = $false
            
            foreach ($pattern in $excludePatterns) {
                if ($file.FullName -like "*\$pattern\*" -or $file.Name -like $pattern) {
                    $shouldExclude = $true
                    break
                }
            }
            
            -not $shouldExclude
        }
        
        Write-Info "Found $($filesToInclude.Count) files to transfer (excluding build artifacts)"
        
        # Create a temporary directory structure
        $tempExtractPath = "$env:TEMP\brightnest-deploy-temp"
        if (Test-Path $tempExtractPath) {
            Remove-Item $tempExtractPath -Recurse -Force
        }
        New-Item -ItemType Directory -Path $tempExtractPath -Force | Out-Null
        
        # Copy files maintaining directory structure
        foreach ($file in $filesToInclude) {
            $relativePath = $file.FullName.Substring($sourcePath.Length + 1)
            $destinationPath = Join-Path $tempExtractPath $relativePath
            $destinationDir = Split-Path $destinationPath -Parent
            
            if (-not (Test-Path $destinationDir)) {
                New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
            }
            
            Copy-Item $file.FullName -Destination $destinationPath -Force
        }
        
        # Compress the temporary directory
        Compress-Archive -Path "$tempExtractPath\*" -DestinationPath $tempZip -Force -CompressionLevel Optimal
        
        # Clean up temporary directory
        Remove-Item $tempExtractPath -Recurse -Force
        
        Pop-Location
        
        # Transfer archive
        Write-Info "Transferring archive to VPS..."
        if (Test-Path $SshKeyPath) {
            scp -i $SshKeyPath $tempZip "$VpsUser@$VpsHost`:$VpsPath/brightnest-deploy.zip"
        } else {
            scp $tempZip "$VpsUser@$VpsHost`:$VpsPath/brightnest-deploy.zip"
        }
        
        # Install unzip if not present and extract on VPS
        Write-Info "Extracting files on VPS..."
        
        # Use single line commands to avoid line ending issues
        if (Test-Path $SshKeyPath) {
            ssh -i $SshKeyPath "$VpsUser@$VpsHost" "command -v unzip >/dev/null || apt-get install -y unzip"
            ssh -i $SshKeyPath "$VpsUser@$VpsHost" "cd $VpsPath && rm -rf Brightness_be && mkdir -p Brightness_be && cd Brightness_be && unzip -o ../brightnest-deploy.zip && cd .. && rm brightnest-deploy.zip"
        } else {
            ssh "$VpsUser@$VpsHost" "command -v unzip >/dev/null || apt-get install -y unzip"
            ssh "$VpsUser@$VpsHost" "cd $VpsPath && rm -rf Brightness_be && mkdir -p Brightness_be && cd Brightness_be && unzip -o ../brightnest-deploy.zip && cd .. && rm brightnest-deploy.zip"
        }
        
        # Clean up
        Remove-Item $tempZip -Force
    }
    
    Write-Success "Files synced successfully"
}

# Deploy on VPS
function Invoke-VpsDeployment {
    Write-Info "Starting deployment on VPS..."
    
    # Use single line command to avoid line ending issues
    if (Test-Path $SshKeyPath) {
        ssh -i $SshKeyPath "$VpsUser@$VpsHost" "cd $VpsPath/Brightness_be/deploy/vps && chmod +x deploy.sh && ./deploy.sh"
    } else {
        ssh "$VpsUser@$VpsHost" "cd $VpsPath/Brightness_be/deploy/vps && chmod +x deploy.sh && ./deploy.sh"
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Deployment completed successfully"
        return $true
    } else {
        Write-Error "Deployment failed"
        return $false
    }
}

# View logs
function Show-Logs {
    Write-Info "Fetching logs from VPS..."
    
    # Use single line command to avoid line ending issues
    if (Test-Path $SshKeyPath) {
        ssh -i $SshKeyPath "$VpsUser@$VpsHost" "cd $VpsPath/Brightness_be && docker-compose -f docker-compose.vps.yml --env-file .env logs --tail=100"
    } else {
        ssh "$VpsUser@$VpsHost" "cd $VpsPath/Brightness_be && docker-compose -f docker-compose.vps.yml --env-file .env logs --tail=100"
    }
}

# Main deployment flow
function Main {
    Write-Info "BrightNest VPS Deployment from Windows"
    Write-Info "========================================"
    Write-Host ""
    Write-Info "VPS: $VpsUser@$VpsHost"
    Write-Info "Path: $VpsPath"
    Write-Host ""
    
    # Test connection
    if (-not (Test-SshConnection)) {
        Write-Error "Cannot proceed without SSH connection"
        exit 1
    }
    
    Write-Host ""
    
    # Ensure directory exists
    if (-not (Ensure-VpsDirectory)) {
        Write-Error "Cannot proceed without VPS directory"
        exit 1
    }
    
    Write-Host ""
    
    # Sync files
    try {
        Sync-Files
    } catch {
        Write-Error "File sync failed: $_"
        exit 1
    }
    
    Write-Host ""
    
    # Deploy
    try {
        $deploySuccess = Invoke-VpsDeployment
        
        if (-not $deploySuccess) {
            Write-Host ""
            Write-Error "Deployment failed. Check the logs above for details."
            Write-Info "You can manually check on VPS: cd /brightnest/Brightness_be/deploy/vps && ls -la"
            exit 1
        }
    } catch {
        Write-Error "Deployment failed: $_"
        exit 1
    }
    
    Write-Host ""
    Write-Success "Deployment completed successfully!"
    Write-Host ""
    Write-Info "Your API should be available at: http://$VpsHost:8088"
    Write-Host ""
    
    # Ask to view logs
    $viewLogs = Read-Host "Do you want to view the logs? (y/n)"
    if ($viewLogs -eq 'y' -or $viewLogs -eq 'Y') {
        Show-Logs
    }
}

# Run main function
try {
    Main
} catch {
    Write-Error "An error occurred: $_"
    exit 1
}

