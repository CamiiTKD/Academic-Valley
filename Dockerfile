# Multi-stage build para imagen mínima de producción
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos solo los .csproj primero para aprovechar el cache de layers de Docker
COPY ["src/AcademicPlanner.API/AcademicPlanner.API.csproj", "src/AcademicPlanner.API/"]
COPY ["src/AcademicPlanner.Application/AcademicPlanner.Application.csproj", "src/AcademicPlanner.Application/"]
COPY ["src/AcademicPlanner.Domain/AcademicPlanner.Domain.csproj", "src/AcademicPlanner.Domain/"]
COPY ["src/AcademicPlanner.Infrastructure/AcademicPlanner.Infrastructure.csproj", "src/AcademicPlanner.Infrastructure/"]

RUN dotnet restore "src/AcademicPlanner.API/AcademicPlanner.API.csproj"

COPY . .

WORKDIR "/src/src/AcademicPlanner.API"
RUN dotnet build "AcademicPlanner.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AcademicPlanner.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AcademicPlanner.API.dll"]
