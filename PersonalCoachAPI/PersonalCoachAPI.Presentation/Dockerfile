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
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalCoachAPI.Presentation.dll"]
