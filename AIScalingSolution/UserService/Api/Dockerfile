FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["UserService/Api/UserService.Api.csproj", "UserService/Api/"]
COPY ["UserService/Application/UserService.Application.csproj", "UserService/Application/"]
COPY ["UserService/Domain/UserService.Domain.csproj", "UserService/Domain/"]
COPY ["UserService/Infrastructure/UserService.Infrastructure.csproj", "UserService/Infrastructure/"]
COPY ["Shared/AIScaling.Shared/AIScaling.Shared.csproj", "Shared/AIScaling.Shared/"]
COPY ["Infrastructure/AIScaling.Infrastructure/AIScaling.Infrastructure.csproj", "Infrastructure/AIScaling.Infrastructure/"]
RUN dotnet restore "UserService/Api/UserService.Api.csproj"
COPY . .
WORKDIR "/src/UserService/Api"
RUN dotnet build "UserService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
