# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["PersonalCoachAPI/PersonalCoachAPI.Presentation/PersonalCoachAPI.Presentation.csproj", "PersonalCoachAPI/PersonalCoachAPI.Presentation/"]
COPY ["PersonalCoachAPI/PersonalCoachAPI.Application/PersonalCoachAPI.Application.csproj", "PersonalCoachAPI/PersonalCoachAPI.Application/"]
COPY ["PersonalCoachAPI/PersonalCoachAPI.Infrastructure/PersonalCoachAPI.Infrastructure.csproj", "PersonalCoachAPI/PersonalCoachAPI.Infrastructure/"]
COPY ["PersonalCoachAPI/PersonalCoachAPI.Domain/PersonalCoachAPI.Domain.csproj", "PersonalCoachAPI/PersonalCoachAPI.Domain/"]

# Restore dependencies
RUN dotnet restore "PersonalCoachAPI/PersonalCoachAPI.Presentation/PersonalCoachAPI.Presentation.csproj"

# Copy everything else
COPY PersonalCoachAPI/ PersonalCoachAPI/

# Build
WORKDIR "/src/PersonalCoachAPI/PersonalCoachAPI.Presentation"
RUN dotnet build "PersonalCoachAPI.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonalCoachAPI.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalCoachAPI.Presentation.dll"]

