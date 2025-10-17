# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NutritionAPI/NutritionAPI.sln NutritionAPI/
COPY NutritionAPI/NutritionAPI.Application NutritionAPI/NutritionAPI.Application
COPY NutritionAPI/NutritionAPI.Domain NutritionAPI/NutritionAPI.Domain
COPY NutritionAPI/NutritionAPI.Infrastructure NutritionAPI/NutritionAPI.Infrastructure
COPY NutritionAPI/NutritionAPI.Presentation NutritionAPI/NutritionAPI.Presentation
COPY BrightNess.SharedLibrarySolution BrightNess.SharedLibrarySolution

RUN dotnet restore NutritionAPI/NutritionAPI.sln
RUN dotnet publish NutritionAPI/NutritionAPI.Presentation/NutritionAPI.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "NutritionAPI.Presentation.dll"]
