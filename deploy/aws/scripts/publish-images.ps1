param(
    [Parameter(Mandatory=$true)]
    [string]$AwsAccountId,

    [string]$Region = "ap-southeast-2",

    [string]$RepositoryPrefix = "brightness-prod",

    [string]$ImageTag = "latest",

    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$services = @(
    @{ Name = "gateway";        Dockerfile = "GatewayAPI/APIGateway.Presentation/Dockerfile" },
    @{ Name = "authentication"; Dockerfile = "AuthenticationAPI/AuthenticationAPI.Presentation/Dockerfile" },
    @{ Name = "measurement";    Dockerfile = "MeasurementAPI/MeasurementAPI.Presentation/Dockerfile" },
    @{ Name = "workout";        Dockerfile = "WorkoutAPI/WorkoutAPI.Presentation/Dockerfile" },
    @{ Name = "nutrition";      Dockerfile = "NutritionAPI/NutritionAPI.Presentation/Dockerfile" },
    @{ Name = "challenge";      Dockerfile = "ChallengeAPI/ChallengeAPI.Presentation/Dockerfile" },
    @{ Name = "social";         Dockerfile = "SocialAPI/SocialAPI.Presentation/Dockerfile" },
    @{ Name = "billing";        Dockerfile = "BillingAPI/BillingAPI.Presentation/Dockerfile" },
    @{ Name = "media";          Dockerfile = "MediaAPI/MediaAPI.Presentation/Dockerfile" },
    @{ Name = "chat";           Dockerfile = "ChatService/ChatService.Presentation/Dockerfile" }
)

$registry = "$AwsAccountId.dkr.ecr.$Region.amazonaws.com"

Write-Host "Logging in to $registry"
aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin $registry

foreach ($svc in $services) {
    $repoName = "$RepositoryPrefix-$($svc.Name)"
    $repositoryUri = "$registry/$repoName"

    Write-Host "Ensuring repository $repoName exists"
    aws ecr describe-repositories --repository-names $repoName --region $Region *> $null
    if ($LASTEXITCODE -ne 0) {
        aws ecr create-repository --repository-name $repoName --region $Region | Out-Null
    }

    if (-not $SkipBuild) {
        Write-Host "Building $repositoryUri:$ImageTag"
        docker build --file $svc.Dockerfile --tag "$repositoryUri:$ImageTag" .
    } else {
        Write-Host "Skipping build for $repositoryUri:$ImageTag" -ForegroundColor Yellow
    }

    Write-Host "Pushing $repositoryUri:$ImageTag"
    docker push "$repositoryUri:$ImageTag"
}
