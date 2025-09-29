# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BillingAPI/BillingAPI.sln BillingAPI/
COPY BillingAPI/BillingAPI.Application BillingAPI/BillingAPI.Application
COPY BillingAPI/BillingAPI.Domain BillingAPI/BillingAPI.Domain
COPY BillingAPI/BillingAPI.Infrastructure BillingAPI/BillingAPI.Infrastructure
COPY BillingAPI/BillingAPI.Presentation BillingAPI/BillingAPI.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore BillingAPI/BillingAPI.sln
RUN dotnet publish BillingAPI/BillingAPI.Presentation/BillingAPI.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8085
EXPOSE 8085
ENTRYPOINT ["dotnet", "BillingAPI.Presentation.dll"]
