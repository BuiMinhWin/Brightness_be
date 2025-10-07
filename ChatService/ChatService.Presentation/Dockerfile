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
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ChatService.Presentation.dll"]
