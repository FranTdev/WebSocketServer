# Interfaces del Núcleo (`Core/Interfaces`)

Este módulo define los contratos formales para la arquitectura desacoplada del servidor, permitiendo sustituir o testear cualquier componente (base de datos, capa de red, enrutador) sin afectar la lógica de negocio.

---

## Contratos Principales

### 1. `IUserRepository.cs`
Abstracción de acceso a datos para usuarios.
- `GetByIdAsync`: Localiza por clave primaria.
- `GetByUsernameAsync`: Búsqueda por índice único de nombre.
- `UpsertAsync`: Registro idempotente o actualización completa.
- `UpdatePositionAsync`: Actualización de alta frecuencia de coordenadas 3D.
- `GetAllAsync`: Listado completo de usuarios.

### 2. `IChatRepository.cs`
Abstracción de almacenamiento de mensajes de chat.
- `SaveMessageAsync`: Persiste un mensaje en el almacenamiento.
- `GetRecentMessagesAsync`: Recupera los últimos $N$ mensajes por canal ordenados en tiempo real.

### 3. `IClientSession.cs`
Encapsula la sesión de un cliente conectado.
- Gestiona el identificador de sesión, el usuario autenticado, el envío seguro de texto y el cierre ordenado de la conexión.

### 4. `IConnectionManager.cs`
Administra el ciclo de vida colectivo de las sesiones activas.
- Soporta registro, desregistro, conteo activo, y difusión de paquetes tanto global (`BroadcastAsync`) como selectiva con exclusión del emisor (`BroadcastExceptAsync`).

### 5. `IPacketHandler.cs`
Implementa el patrón Command/Strategy para el procesamiento desacoplado de códigos de operación de red (`OpCode`).
