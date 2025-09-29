# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MediaAPI/MediaAPI.sln MediaAPI/
COPY MediaAPI/MediaAPI.Application MediaAPI/MediaAPI.Application
COPY MediaAPI/MediaAPI.Domain MediaAPI/MediaAPI.Domain
COPY MediaAPI/MediaAPI.Infrastructure MediaAPI/MediaAPI.Infrastructure
COPY MediaAPI/MediaAPI.Presentation MediaAPI/MediaAPI.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore MediaAPI/MediaAPI.sln
RUN dotnet publish MediaAPI/MediaAPI.Presentation/MediaAPI.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8086
EXPOSE 8086
ENTRYPOINT ["dotnet", "MediaAPI.Presentation.dll"]
