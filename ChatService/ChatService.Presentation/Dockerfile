
# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ChatService/ChatService.sln ChatService/
COPY ChatService/ChatService.Application ChatService/ChatService.Application
COPY ChatService/ChatService.Domain ChatService/ChatService.Domain
COPY ChatService/ChatService.Infrastructure ChatService/ChatService.Infrastructure
COPY ChatService/ChatService.Presentation ChatService/ChatService.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore ChatService/ChatService.sln
RUN dotnet publish ChatService/ChatService.Presentation/ChatService.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

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
COPY --from=build /app/publish .

# Change ownership of the entire /app directory to the app user
RUN chown -R app:app /app

# Now switch to the non-root user for execution
USER app

EXPOSE 8080
ENTRYPOINT ["/app/entrypoint.sh"]
CMD ["dotnet", "ChatService.Presentation.dll"]
