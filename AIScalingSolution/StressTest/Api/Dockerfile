FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y libgomp1 libomp-dev && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["StressTest/Api/StressTest.Api.csproj", "StressTest/Api/"]
COPY ["Infrastructure/AIScaling.PredictiveMiddleware/AIScaling.PredictiveMiddleware.csproj", "Infrastructure/AIScaling.PredictiveMiddleware/"]
RUN dotnet restore "StressTest/Api/StressTest.Api.csproj"
COPY . .
WORKDIR "/src/StressTest/Api"
RUN dotnet publish "StressTest.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "StressTest.Api.dll"]
