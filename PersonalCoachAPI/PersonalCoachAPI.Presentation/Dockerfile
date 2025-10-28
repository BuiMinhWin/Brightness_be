# syntax=docker/dockerfile:1
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first to leverage restore cache
COPY PersonalCoachAPI/PersonalCoachAPI.sln PersonalCoachAPI/
COPY PersonalCoachAPI/PersonalCoachAPI.Application/PersonalCoachAPI.Application.csproj PersonalCoachAPI/PersonalCoachAPI.Application/
COPY PersonalCoachAPI/PersonalCoachAPI.Domain/PersonalCoachAPI.Domain.csproj PersonalCoachAPI/PersonalCoachAPI.Domain/
COPY PersonalCoachAPI/PersonalCoachAPI.Infrastructure/PersonalCoachAPI.Infrastructure.csproj PersonalCoachAPI/PersonalCoachAPI.Infrastructure/
COPY PersonalCoachAPI/PersonalCoachAPI.Presentation/PersonalCoachAPI.Presentation.csproj PersonalCoachAPI/PersonalCoachAPI.Presentation/
COPY BrightNess.SharedLibrarySolution/BrightNess.Library/BrightNess.Library.csproj BrightNess.SharedLibrarySolution/BrightNess.Library/

RUN dotnet restore PersonalCoachAPI/PersonalCoachAPI.sln

# Copy the rest of the source now that restore is cached
COPY PersonalCoachAPI/ PersonalCoachAPI/
COPY BrightNess.SharedLibrarySolution/ BrightNess.SharedLibrarySolution/

RUN dotnet publish PersonalCoachAPI/PersonalCoachAPI.Presentation/PersonalCoachAPI.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=build /app/publish .
# Copy SeedData folder for database seeding
COPY PersonalCoachAPI/SeedData /app/SeedData

EXPOSE 8080
ENTRYPOINT ["dotnet", "PersonalCoachAPI.Presentation.dll"]
