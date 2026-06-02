FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PredictionService/Api/PredictionService.Api.csproj", "PredictionService/Api/"]
COPY ["PredictionService/Application/PredictionService.Application.csproj", "PredictionService/Application/"]
COPY ["PredictionService/Domain/PredictionService.Domain.csproj", "PredictionService/Domain/"]
COPY ["PredictionService/Infrastructure/PredictionService.Infrastructure.csproj", "PredictionService/Infrastructure/"]
COPY ["Shared/AIScaling.Shared/AIScaling.Shared.csproj", "Shared/AIScaling.Shared/"]
COPY ["Infrastructure/AIScaling.Infrastructure/AIScaling.Infrastructure.csproj", "Infrastructure/AIScaling.Infrastructure/"]
RUN dotnet restore "PredictionService/Api/PredictionService.Api.csproj"
COPY . .
WORKDIR "/src/PredictionService/Api"
RUN dotnet publish "PredictionService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PredictionService.Api.dll"]
