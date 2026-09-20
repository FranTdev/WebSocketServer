# ===================================================================
# STAGE 1: Compilación del Servidor (.NET 10 SDK en Alpine Linux)
# ===================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copiar solo el archivo de proyecto para aprovechar la caché de capas en restore
COPY WebSocketServer.csproj ./
RUN dotnet restore WebSocketServer.csproj

# Copiar el código fuente
COPY . ./

# Compilar y publicar binarios optimizados en modo Release
RUN dotnet publish WebSocketServer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ===================================================================
# STAGE 2: Imagen de Ejecución Ultraligera (Alpine Linux Runtime)
# ===================================================================
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final
WORKDIR /app

# Instalar dependencias esenciales en Alpine (ICU para globalización y soporte nativo SQLite)
RUN apk add --no-cache icu-libs sqlite-libs

# Variables de entorno para bajo consumo, diagnóstico deshabilitado y binding global
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_EnableDiagnostics=0 \
    LISTENER_PREFIX=http://*:8080/ \
    DATABASE_PATH=/app/data/GameServerData.db

# Crear directorio para persistencia de la base de datos y asignar permisos al usuario 'app'
RUN mkdir -p /app/data && chown -R app:app /app

# Copiar los artefactos publicados
COPY --from=build --chown=app:app /app/publish .

# Exponer el puerto WebSocket
EXPOSE 8080

# Volumen persistente para la base de datos SQLite WAL
VOLUME ["/app/data"]

# Ejecutar con el usuario seguro no-root 'app'
USER app

ENTRYPOINT ["dotnet", "WebSocketServer.dll"]
