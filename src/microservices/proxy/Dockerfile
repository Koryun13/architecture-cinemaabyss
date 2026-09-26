# Proxy service — Strangler Fig API Gateway (YARP).
#
# The build context is this folder, as set in docker-compose.yml and in the
# docker-build-push workflow:
#
#   docker build -t cinemaabyss/proxy-service src/microservices/proxy
#
# Both stages use Linux-only image tags (-noble = Ubuntu 24.04); the floating
# "10.0" tags also resolve to Windows Nano Server on a Windows daemon, which
# this image must never do.

# ------------------------------------------------------------------------------
# Build
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Project file first: restore is cached until a dependency actually changes.
COPY CinemaAbyss.Proxy.csproj ./
RUN dotnet restore "CinemaAbyss.Proxy.csproj"

# Then the sources, so a code change does not invalidate the restore layer.
COPY . .
RUN dotnet publish "CinemaAbyss.Proxy.csproj" \
        --configuration Release \
        --no-restore \
        --output /app/publish \
        /p:UseAppHost=false

# ------------------------------------------------------------------------------
# Runtime
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final

# PORT is the contract of docker-compose.yml and the Kubernetes manifests;
# the service binds to it on start (see ServiceDefaults.UseServicePort).
ENV PORT=8000 \
    DOTNET_gcServer=1

# curl: used by the HEALTHCHECK below; the image ships neither curl nor wget.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8000
USER $APP_UID

HEALTHCHECK --interval=10s --timeout=5s --retries=5 --start-period=15s \
    CMD curl -fsS "http://localhost:${PORT}/health" || exit 1

ENTRYPOINT ["dotnet", "CinemaAbyss.Proxy.dll"]
