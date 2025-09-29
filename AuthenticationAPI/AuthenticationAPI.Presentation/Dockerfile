
# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AuthenticationAPI/AuthenticationAPI.sln AuthenticationAPI/
COPY AuthenticationAPI/AuthenticationAPI.Application AuthenticationAPI/AuthenticationAPI.Application
COPY AuthenticationAPI/AuthenticationAPI.Domain AuthenticationAPI/AuthenticationAPI.Domain
COPY AuthenticationAPI/AuthenticationAPI.Infrastructure AuthenticationAPI/AuthenticationAPI.Infrastructure
COPY AuthenticationAPI/AuthenticationAPI.Presentation AuthenticationAPI/AuthenticationAPI.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore AuthenticationAPI/AuthenticationAPI.sln
RUN dotnet publish AuthenticationAPI/AuthenticationAPI.Presentation/AuthenticationAPI.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "AuthenticationAPI.Presentation.dll"]
