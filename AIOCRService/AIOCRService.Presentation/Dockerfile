# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép riêng các file dự án để tối ưu cache cho bước restore
COPY AIOCRService/AIOCRService.sln AIOCRService/
COPY AIOCRService/AIOCRService.Application/AIOCRService.Application.csproj AIOCRService/AIOCRService.Application/
COPY AIOCRService/AIOCRService.Domain/AIOCRService.Domain.csproj AIOCRService/AIOCRService.Domain/
COPY AIOCRService/AIOCRService.Infrastructure/AIOCRService.Infrastructure.csproj AIOCRService/AIOCRService.Infrastructure/
COPY AIOCRService/AIOCRService.Presentation/AIOCRService.Presentation.csproj AIOCRService/AIOCRService.Presentation/
COPY BrightNess.SharedLibrarySolution/BrightNess.Library/BrightNess.Library.csproj BrightNess.SharedLibrarySolution/BrightNess.Library/

RUN dotnet restore AIOCRService/AIOCRService.sln

# Sau khi restore, copy toàn bộ mã nguồn
COPY AIOCRService/ AIOCRService/
COPY BrightNess.SharedLibrarySolution/ BrightNess.SharedLibrarySolution/

RUN dotnet publish AIOCRService/AIOCRService.Presentation/AIOCRService.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "AIOCRService.Presentation.dll"]
