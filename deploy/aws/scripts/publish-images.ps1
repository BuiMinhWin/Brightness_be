param(
    [Parameter(Mandatory = $true)]
    [string]$AwsAccountId,

    [string]$Region = "ap-southeast-2",

    [string]$RepositoryPrefix = "brightness-prod",

    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$ImageTag = "latest"

$services = @(
    @{ Name = "gateway-api"; Dockerfile = "GatewayAPI/APIGateway.Presentation/Dockerfile" },
    @{ Name = "authentication-api"; Dockerfile = "AuthenticationAPI/AuthenticationAPI.Presentation/Dockerfile" },
    @{ Name = "measurement-api"; Dockerfile = "MeasurementAPI/MeasurementAPI.Presentation/Dockerfile" },
    @{ Name = "challenge-api"; Dockerfile = "ChallengeAPI/ChallengeAPI.Presentation/Dockerfile" },
    @{ Name = "social-api"; Dockerfile = "SocialAPI/SocialAPI.Presentation/Dockerfile" },
    @{ Name = "billing-api"; Dockerfile = "BillingAPI/BillingAPI.Presentation/Dockerfile" },
    @{ Name = "media-api"; Dockerfile = "MediaAPI/MediaAPI.Presentation/Dockerfile" },
    @{ Name = "chat-service"; Dockerfile = "ChatService/ChatService.Presentation/Dockerfile" },
    @{ Name = "ai-ocr-service"; Dockerfile = "AIOCRService/AIOCRService.Presentation/Dockerfile" },
    @{ Name = "personal-coach-api"; Dockerfile = "PersonalCoachAPI/PersonalCoachAPI.Presentation/Dockerfile" }
)

$registry = "$AwsAccountId.dkr.ecr.$Region.amazonaws.com"

Write-Host "Logging in to $registry"
aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin $registry

foreach ($svc in $services) {
    $repoName = "$RepositoryPrefix-$($svc.Name)"
    $repositoryUri = "$registry/$repoName"

    Write-Host "Ensuring repository $repoName exists"

    $repoExists = $false
    $originalErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $describeOutput = & aws ecr describe-repositories --repository-names $repoName --region $Region --no-cli-pager 2>&1
        $describeExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $originalErrorAction
    }
    $describeMessage = ($describeOutput | Out-String).Trim()

    if ($describeExitCode -eq 0) {
        $repoExists = $true
    }
    elseif ($describeMessage -match "RepositoryNotFoundException") {
        $repoExists = $false
    }
    else {
        throw "Không thể gọi aws ecr describe-repositories cho $repoName. Mã lỗi: $describeExitCode. Chi tiết: $describeMessage"
    }
    
    if (-not $repoExists) {
        Write-Host "Repository $repoName not found. Creating..." -ForegroundColor Yellow
        aws ecr create-repository --repository-name $repoName --region $Region | Out-Null
    }
    else {
        Write-Host "Repository $repoName already exists." -ForegroundColor Green
    }
    if (-not $SkipBuild) {
        Write-Host "Building ${repositoryUri}:${ImageTag}"
        docker build --file $svc.Dockerfile --tag "${repositoryUri}:${ImageTag}" .
    }
    else {
        Write-Host "Skipping build for ${repositoryUri}:${ImageTag}" -ForegroundColor Yellow
    }

    Write-Host "Pushing ${repositoryUri}:${ImageTag}"
    docker push "${repositoryUri}:${ImageTag}"
}
