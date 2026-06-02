FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["OrderService/Api/OrderService.Api.csproj", "OrderService/Api/"]
COPY ["OrderService/Application/OrderService.Application.csproj", "OrderService/Application/"]
COPY ["OrderService/Domain/OrderService.Domain.csproj", "OrderService/Domain/"]
COPY ["OrderService/Infrastructure/OrderService.Infrastructure.csproj", "OrderService/Infrastructure/"]
COPY ["Shared/AIScaling.Shared/AIScaling.Shared.csproj", "Shared/AIScaling.Shared/"]
COPY ["Infrastructure/AIScaling.Infrastructure/AIScaling.Infrastructure.csproj", "Infrastructure/AIScaling.Infrastructure/"]
RUN dotnet restore "OrderService/Api/OrderService.Api.csproj"
COPY . .
WORKDIR "/src/OrderService/Api"
RUN dotnet publish "OrderService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
