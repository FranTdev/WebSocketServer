# Servidor de Videojuegos Multijugador (.NET WebSockets Nativos)

Servidor de alto rendimiento para videojuegos multijugador en tiempo real construido sobre WebSockets nativos de .NET (`HttpListener` y `WebSocket`) y una base de datos embebida ultraligera en SQLite optimizada con **Write-Ahead Logging (WAL)**.

---

## Características Principales

- **Arquitectura Limpia & Modular**: Desacoplamiento estricto en capas independientes:
  - `Configuration`: Opciones operativas fuertemente tipadas.
  - `Core`: Entidades de dominio (`User`, `ChatMessage`, `Vector3Dto`) y contratos (`IUserRepository`, `IChatRepository`, `IConnectionManager`, `IPacketHandler`).
  - `Protocol`: Envoltorios de red tipo sobre (*Envelopes*), catálogo de códigos de operación (`OpCodes`) y DTOs serializados con `System.Text.Json`.
  - `Persistence`: Motor SQLite de alta velocidad con modo WAL, transacciones atómicas y protección total contra inyecciones SQL con consultas parametrizadas.
  - `Network`: Sincronización thread-safe de envíos concurrentes mediante `SemaphoreSlim(1,1)` y bucle de lectura sin bloqueos.
  - `Services`: Casos de uso de autenticación, presencia de jugadores, persistencia y difusión de chat y replicación espacial.
- **Documentación XML 100% Exhaustiva**: Cada clase, método, interfaz, propiedad y parámetro documentado bajo el estándar XML de C#.
- **Zero Dependencias Externas Innecesarias**: Uso exclusivo de APIs nativas de .NET y el proveedor oficial `Microsoft.Data.Sqlite`.

---

## Estructura del Proyecto

```
WebSocketServer/
├── Configuration/               # Configuración del servidor y parámetros de red
├── Core/
│   ├── Models/                 # Entidades de dominio (User, ChatMessage, Vector3Dto, UserRole)
│   └── Interfaces/             # Contratos abstractos para persistencia y red
├── Protocol/
│   ├── Packets/                # DTOs de autenticación, movimiento, chat y sistema
│   └── Serialization/          # Serializador JSON nativo
├── Persistence/                # SQLite WAL, fábrica de conexiones y repositorios
├── Network/                    # Listener HTTP/WS, sesiones y enrutador de paquetes
├── Services/
│   ├── Handlers/               # Manejadores individuales por código de operación (SRP)
│   └── ...                     # Lógica de dominio de usuario y chat
├── Program.cs                  # Composition Root y punto de entrada
└── Assets/Scripts/Networking/  # Scripts desacoplados y listos para Unity Client
```

---

## Ejecución del Servidor

### Requisitos
- SDK de .NET 8.0, 9.0 o 10.0 instalado.

### Compilar
```bash
dotnet build
```

### Ejecutar Localmente (.NET SDK)
```bash
dotnet run
```

Al iniciar, el servidor creará automáticamente la base de datos `GameServerData.db` con las tablas `Users` y `ChatMessages`, optimizará el motor a modo WAL y comenzará a escuchar conexiones WebSocket en `http://localhost:8080/`.

---

## Despliegue con Docker (Alpine Linux)

El proyecto incluye soporte nativo para contenedores ultra livianos basados en **Alpine Linux** con compilación multi-etapa y persistencia de datos.

### 1. Despliegue Automático con Docker Compose (Recomendado)
```bash
# Iniciar el servicio en segundo plano (crea el volumen local ./data automáticamente)
docker compose up -d

# Ver los logs en tiempo real
docker compose logs -f

# Detener el servicio
docker compose down
```

### 2. Construcción y Ejecución Manual con Docker CLI
```bash
# Construir la imagen optimizada
docker build -t websocket-server:latest .

# Ejecutar el contenedor con persistencia de base de datos y mapeo de puerto
docker run -d \
  --name websocket-game-server \
  --restart unless-stopped \
  -p 8080:8080 \
  -v "$(pwd)/data:/app/data" \
  websocket-server:latest
```

---

## Guía de Códigos de Operación (OpCodes)

| OpCode | Nombre | Dirección | Descripción |
| :--- | :--- | :--- | :--- |
| `1` | `Ping` | Cliente ➔ Servidor | Verificación de latencia / latido |
| `2` | `Pong` | Servidor ➔ Cliente | Respuesta inmediata a Ping |
| `100` | `AuthRequest` | Cliente ➔ Servidor | Solicitud de inicio de sesión / registro |
| `101` | `AuthResponse` | Servidor ➔ Cliente | Confirmación de usuario y spawn 3D |
| `200` | `UserJoined` | Servidor ➔ Clientes | Notificación de nuevo jugador conectado |
| `201` | `UserLeft` | Servidor ➔ Clientes | Notificación de jugador desconectado |
| `202` | `UserListSync` | Servidor ➔ Cliente | Lista de jugadores actualmente en línea |
| `300` | `PositionUpdate` | Bidireccional | Coordenadas espaciales 3D del jugador |
| `400` | `ChatSendMessage` | Cliente ➔ Servidor | Envío de mensaje a un canal |
| `401` | `ChatBroadcast` | Servidor ➔ Clientes | Difusión de mensaje validado |
| `402` | `ChatHistory` | Servidor ➔ Cliente | Historial reciente de mensajes |
| `500` | `ErrorNotification`| Servidor ➔ Cliente | Mensaje de error tipado |
| `501` | `SystemAnnouncement`| Servidor ➔ Clientes| Anuncio administrativo global |
