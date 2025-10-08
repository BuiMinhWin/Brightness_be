# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ChallengeAPI/ChallengeAPI.sln ChallengeAPI/
COPY ChallengeAPI/ChallengeAPI.Application ChallengeAPI/ChallengeAPI.Application
COPY ChallengeAPI/ChallengeAPI.Domain ChallengeAPI/ChallengeAPI.Domain
COPY ChallengeAPI/ChallengeAPI.Infrastructure ChallengeAPI/ChallengeAPI.Infrastructure
COPY ChallengeAPI/ChallengeAPI.Presentation ChallengeAPI/ChallengeAPI.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore ChallengeAPI/ChallengeAPI.sln
RUN dotnet publish ChallengeAPI/ChallengeAPI.Presentation/ChallengeAPI.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ChallengeAPI.Presentation.dll"]
