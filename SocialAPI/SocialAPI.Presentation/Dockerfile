# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SocialAPI/SocialAPI.sln SocialAPI/
COPY SocialAPI/SocialAPI.Application SocialAPI/SocialAPI.Application
COPY SocialAPI/SocialAPI.Domain SocialAPI/SocialAPI.Domain
COPY SocialAPI/SocialAPI.Infrastructure SocialAPI/SocialAPI.Infrastructure
COPY SocialAPI/SocialAPI.Presentation SocialAPI/SocialAPI.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore SocialAPI/SocialAPI.sln
RUN dotnet publish SocialAPI/SocialAPI.Presentation/SocialAPI.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8084
EXPOSE 8084
ENTRYPOINT ["dotnet", "SocialAPI.Presentation.dll"]
