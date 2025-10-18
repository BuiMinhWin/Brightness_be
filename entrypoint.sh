#!/bin/sh
# Exit immediately if a command exits with a non-zero status.
set -e

# Check the environment
if [ "${ASPNETCORE_ENVIRONMENT}" = "Production" ]; then
  # --- PRODUCTION MODE ---
  echo "PRODUCTION MODE: Attempting to fetch secrets from AWS Parameter Store..."

  if ! command -v aws >/dev/null 2>&1; then
    echo "AWS CLI not found in container. Skipping secret fetching; using existing environment variables."
  elif [ -z "${AWS_DEFAULT_REGION:-}" ]; then
    echo "AWS_DEFAULT_REGION is not set. Skipping secret fetching; using existing environment variables."
  elif ! aws --no-cli-pager sts get-caller-identity >/dev/null 2>&1; then
    echo "AWS credentials not detected. Skipping secret fetching; using existing environment variables."
  else
    PARAM_TEMP_JSON="$(mktemp)"
    PARAM_TEMP_ENV="$(mktemp)"

    if aws --no-cli-pager ssm get-parameters-by-path \
      --path "/Brightness/Production" \
      --with-decryption \
      --query "Parameters" > "${PARAM_TEMP_JSON}"
    then
      jq -r '.[] | .Name + "=" + .Value' "${PARAM_TEMP_JSON}" | \
        sed 's|/Brightness/Production/||' > "${PARAM_TEMP_ENV}"

      # Source the secrets into the environment
      . "${PARAM_TEMP_ENV}"

      echo "Secrets fetched and environment variables set."
    else
      echo "Failed to fetch secrets from AWS Parameter Store. Using existing environment variables."
    fi

    rm -f "${PARAM_TEMP_JSON}" "${PARAM_TEMP_ENV}"
  fi

  # Construct the main database connection string from the sourced variables
  if [ -n "${DB_HOST:-}" ] && [ -n "${DB_PORT:-}" ] && [ -n "${DB_NAME:-}" ] && [ -n "${DB_USER:-}" ] && [ -n "${DB_PASSWORD:-}" ]; then
    export ConnectionStrings__BrightnessDb="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};IncludeErrorDetail=true"
    # Personal Coach API uses DefaultConnection; align it with the shared DB string when available.
    export ConnectionStrings__DefaultConnection="${ConnectionStrings__BrightnessDb}"
  else
    echo "Warning: Database variables are incomplete; falling back to existing connection string values."
  fi

  # Construct the redis connection string
  if [ -n "${REDIS_PASSWORD:-}" ]; then
    export ConnectionStrings__Redis="${REDIS_HOST:-redis}:${REDIS_PORT:-6379},password=${REDIS_PASSWORD}"
  else
    echo "Warning: REDIS_PASSWORD not set; Redis connection string will use existing environment value."
  fi
else
  # --- DEVELOPMENT MODE ---
  echo "DEVELOPMENT MODE: Skipping AWS secret fetching. Using local docker-compose variables."
fi

# Execute the command passed to this script (which is the .NET application's CMD)
echo "Starting application..."
exec "$@"
