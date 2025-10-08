FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AIOCRService/AIOCRService.Presentation/AIOCRService.Presentation.csproj", "AIOCRService/AIOCRService.Presentation/"]
COPY ["AIOCRService/AIOCRService.Application/AIOCRService.Application.csproj", "AIOCRService/AIOCRService.Application/"]
COPY ["AIOCRService/AIOCRService.Infrastructure/AIOCRService.Infrastructure.csproj", "AIOCRService/AIOCRService.Infrastructure/"]
COPY ["AIOCRService/AIOCRService.Domain/AIOCRService.Domain.csproj", "AIOCRService/AIOCRService.Domain/"]
RUN dotnet restore "AIOCRService/AIOCRService.Presentation/AIOCRService.Presentation.csproj"
COPY . .
WORKDIR "/src/AIOCRService/AIOCRService.Presentation"
RUN dotnet build "AIOCRService.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/AIOCRService/AIOCRService.Presentation"
RUN dotnet publish "AIOCRService.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIOCRService.Presentation.dll"]

