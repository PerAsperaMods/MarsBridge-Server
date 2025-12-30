# MarsBridge Server ASP.NET Core Application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["MarsBridge.Server.csproj", "."]
RUN dotnet restore "MarsBridge.Server.csproj"

# Copy source code
COPY . .
WORKDIR "/src"
RUN dotnet build "MarsBridge.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarsBridge.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MarsBridge.Server.dll"]