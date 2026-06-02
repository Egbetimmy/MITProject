FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MonitoringService/Api/MonitoringService.Api.csproj", "MonitoringService/Api/"]
COPY ["MonitoringService/Application/MonitoringService.Application.csproj", "MonitoringService/Application/"]
COPY ["MonitoringService/Domain/MonitoringService.Domain.csproj", "MonitoringService/Domain/"]
COPY ["MonitoringService/Infrastructure/MonitoringService.Infrastructure.csproj", "MonitoringService/Infrastructure/"]
COPY ["Shared/AIScaling.Shared/AIScaling.Shared.csproj", "Shared/AIScaling.Shared/"]
COPY ["Infrastructure/AIScaling.Infrastructure/AIScaling.Infrastructure.csproj", "Infrastructure/AIScaling.Infrastructure/"]
RUN dotnet restore "MonitoringService/Api/MonitoringService.Api.csproj"
COPY . .
WORKDIR "/src/MonitoringService/Api"
RUN dotnet publish "MonitoringService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MonitoringService.Api.dll"]
