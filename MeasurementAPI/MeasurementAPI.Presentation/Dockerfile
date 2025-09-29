# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MeasurementAPI/MeasurementAPI.sln MeasurementAPI/
COPY MeasurementAPI/MeasurementAPI.Application MeasurementAPI/MeasurementAPI.Application
COPY MeasurementAPI/MeasurementAPI.Domain MeasurementAPI/MeasurementAPI.Domain
COPY MeasurementAPI/MeasurementAPI.Infrastructure MeasurementAPI/MeasurementAPI.Infrastructure
COPY MeasurementAPI/MeasurementAPI.Presentation MeasurementAPI/MeasurementAPI.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore MeasurementAPI/MeasurementAPI.sln
RUN dotnet publish MeasurementAPI/MeasurementAPI.Presentation/MeasurementAPI.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MeasurementAPI.Presentation.dll"]
