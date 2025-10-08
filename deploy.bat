@echo off
SETLOCAL

REM Check for environment argument (dev or prod)
IF /I "%1"=="dev" (
    echo "--- Starting DEVELOPMENT environment ---"
    docker-compose -f docker-compose.yml up -d --build
    GOTO :EOF
)

IF /I "%1"=="prod" (
    echo "--- Starting PRODUCTION environment ---"
    echo "Using docker-compose.yml and docker-compose.prod.yml"
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
    GOTO :EOF
)

REM If no valid argument is provided
echo "Error: Invalid or missing environment specified."
echo "Usage: deploy.bat [dev^|prod]"
EXIT /B 1
