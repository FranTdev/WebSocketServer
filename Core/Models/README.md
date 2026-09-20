# Entidades del Núcleo (`Core/Models`)

Este módulo contiene las entidades de dominio y tipos de datos esenciales del servidor de juego, totalmente desacoplados de cualquier mecanismo de transporte o base de datos específica.

---

## Modelos Incluidos

### 1. `User.cs`
Entidad principal del jugador o usuario.
- `Id`: Identificador único UUID inmutable.
- `Username`: Nombre de usuario registrado o conectado.
- `Role`: Nivel de permisos (`Guest`, `Player`, `Moderator`, `Admin`).
- `Position`: Vector espacial tridimensional (`Vector3Dto`).
- `CreatedAt` y `LastActiveAt`: Métricas de ciclo de vida del usuario.

### 2. `UserRole.cs`
Enumeración que rige las políticas de autorización, evitando cadenas mágicas en la validación de comandos y privilegios.

### 3. `Vector3Dto.cs`
Estructura inmutable de posición tridimensional `(X, Y, Z)` de alto rendimiento.
- Proporciona cálculo optimizado de distancias (`SqrDistance`) que omite raíces cuadradas para comprobaciones rápidas de proximidad.
- Implementa `IEquatable<Vector3Dto>` para evitar boxing en colecciones genéricas.

### 4. `ChatMessage.cs`
Entidad para el registro de mensajes de chat en tiempo real.
- Incluye soporte para canales (`Global`, `System`, `Whisper`), marcas de tiempo Unix en milisegundos y metadatos del emisor.
