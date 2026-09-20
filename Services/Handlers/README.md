# Manejadores de Paquetes (`Services/Handlers`)

Este subdirectorio contiene los manejadores individuales de paquetes basados en el patrón *Command/Handler* y el principio de responsabilidad única (*Single Responsibility Principle - SRP*). Cada clase procesa un único código de operación de red (`OpCode`).

---

## Catálogo de Manejadores

### 1. `AuthPacketHandler.cs` (`OpCode 100`)
- Procesa peticiones de login/registro.
- Asocia la entidad `User` a la sesión de WebSocket.
- Difunde la presencia del nuevo usuario al resto de jugadores y le entrega el listado de jugadores ya conectados.
- Remite de forma inmediata los últimos mensajes registrados en el canal de chat.

### 2. `PositionPacketHandler.cs` (`OpCode 300`)
- Comprueba que la sesión esté debidamente autenticada.
- Actualiza las coordenadas espaciales `(X, Y, Z)` tanto en memoria como en la base de datos.
- Reenvía el paquete a todos los clientes concurrentes omitiendo al emisor para no consumir ancho de banda innecesario.

### 3. `ChatPacketHandler.cs` (`OpCode 400`)
- Valida la longitud y contenido del mensaje.
- Invoca a `ChatService` para almacenar el mensaje en SQLite y difundir el evento de chat enriquecido con rol, nombre y marca temporal.
