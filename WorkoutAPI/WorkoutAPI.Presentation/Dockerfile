# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WorkoutAPI/WorkoutAPI.sln WorkoutAPI/
COPY WorkoutAPI/WorkoutAPI.Application WorkoutAPI/WorkoutAPI.Application
COPY WorkoutAPI/WorkoutAPI.Domain WorkoutAPI/WorkoutAPI.Domain
COPY WorkoutAPI/WorkoutAPI.Infrastructure WorkoutAPI/WorkoutAPI.Infrastructure
COPY WorkoutAPI/WorkoutAPI.Presentation WorkoutAPI/WorkoutAPI.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore WorkoutAPI/WorkoutAPI.sln
RUN dotnet publish WorkoutAPI/WorkoutAPI.Presentation/WorkoutAPI.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WorkoutAPI.Presentation.dll"]
