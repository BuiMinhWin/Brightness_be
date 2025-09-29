# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GatewayAPI/GatewayAPI.sln GatewayAPI/
COPY GatewayAPI/APIGateway.Presentation GatewayAPI/APIGateway.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore GatewayAPI/GatewayAPI.sln
RUN dotnet publish GatewayAPI/APIGateway.Presentation/APIGateway.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8088
EXPOSE 8088
ENTRYPOINT ["dotnet", "APIGateway.Presentation.dll"]
