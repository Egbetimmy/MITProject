FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ProductService/Api/ProductService.Api.csproj", "ProductService/Api/"]
COPY ["ProductService/Application/ProductService.Application.csproj", "ProductService/Application/"]
COPY ["ProductService/Domain/ProductService.Domain.csproj", "ProductService/Domain/"]
COPY ["ProductService/Infrastructure/ProductService.Infrastructure.csproj", "ProductService/Infrastructure/"]
COPY ["Shared/AIScaling.Shared/AIScaling.Shared.csproj", "Shared/AIScaling.Shared/"]
COPY ["Infrastructure/AIScaling.Infrastructure/AIScaling.Infrastructure.csproj", "Infrastructure/AIScaling.Infrastructure/"]
RUN dotnet restore "ProductService/Api/ProductService.Api.csproj"
COPY . .
WORKDIR "/src/ProductService/Api"
RUN dotnet publish "ProductService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
