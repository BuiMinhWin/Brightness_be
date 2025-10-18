# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["PersonalCoachAPI/PersonalCoachAPI.Presentation/PersonalCoachAPI.Presentation.csproj", "PersonalCoachAPI/PersonalCoachAPI.Presentation/"]
COPY ["PersonalCoachAPI/PersonalCoachAPI.Application/PersonalCoachAPI.Application.csproj", "PersonalCoachAPI/PersonalCoachAPI.Application/"]
COPY ["PersonalCoachAPI/PersonalCoachAPI.Domain/PersonalCoachAPI.Domain.csproj", "PersonalCoachAPI/PersonalCoachAPI.Domain/"]
COPY ["PersonalCoachAPI/PersonalCoachAPI.Infrastructure/PersonalCoachAPI.Infrastructure.csproj", "PersonalCoachAPI/PersonalCoachAPI.Infrastructure/"]

# Restore packages
RUN dotnet restore "PersonalCoachAPI/PersonalCoachAPI.Presentation/PersonalCoachAPI.Presentation.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/PersonalCoachAPI/PersonalCoachAPI.Presentation"
RUN dotnet build "PersonalCoachAPI.Presentation.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "PersonalCoachAPI.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Perform all privileged operations as root first
USER root
WORKDIR /app

# Install dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    unzip \
    jq \
    dos2unix \
    && rm -rf /var/lib/apt/lists/*

# Install AWS CLI
RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" \
    && unzip awscliv2.zip \
    && ./aws/install \
    && rm -rf awscliv2.zip aws

# Copy and prepare scripts and application files
COPY entrypoint.sh .
RUN dos2unix entrypoint.sh && chmod +x entrypoint.sh
COPY --from=publish /app/publish .
COPY ["PersonalCoachAPI/SeedData", "SeedData/"]

# Change ownership of the entire /app directory to the app user
RUN chown -R app:app /app

# Now switch to the non-root user for execution
USER app

EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
CMD ["dotnet", "PersonalCoachAPI.Presentation.dll"]
