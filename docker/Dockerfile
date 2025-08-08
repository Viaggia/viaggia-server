# ===== STAGE 1: build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas o csproj para melhorar cache
COPY viaggia-server/viaggia-server.csproj viaggia-server/
RUN dotnet restore viaggia-server/viaggia-server.csproj

# Copia o restante do código do projeto
COPY viaggia-server/ viaggia-server/

# Build + Publish
RUN dotnet build viaggia-server/viaggia-server.csproj -c Release --no-restore
RUN dotnet publish viaggia-server/viaggia-server.csproj -c Release -o /out --no-restore

# ===== STAGE 2: runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# (Opcional) curl para healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Metadados (versão)
ARG APP_VERSION=0.1.1
LABEL org.opencontainers.image.title="viaggia-server" \
      org.opencontainers.image.version="${APP_VERSION:-0.1.1}" \
      org.opencontainers.image.description="Viaggia Server API"

# Artefatos publicados
COPY --from=build /out ./

# Porta/ambiente
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Healthcheck no seu endpoint /health
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -fsS http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "viaggia-server.dll"]
