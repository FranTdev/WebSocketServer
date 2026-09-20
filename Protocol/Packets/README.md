# Cargas Útiles del Protocolo (`Protocol/Packets`)

Este subdirectorio define los objetos de transferencia de datos (DTOs) que viajan dentro del campo `payload` de la envoltura `NetworkPacket`.

---

## Catálogo de DTOs

### 1. Autenticación y Presencia (`AuthPackets.cs`)
- **`AuthRequestPayload`**: Enviado por el cliente con `username` y `role` deseado.
- **`AuthResponsePayload`**: Respuesta del servidor confirmando éxito, `userId`, posición de aparición (`spawnPosition`) y errores.
- **`UserSyncPayload`**: Notifica el estado de jugadores conectados para instanciación o actualización en el cliente.

### 2. Posicionamiento Espacial (`PositionPackets.cs`)
- **`PositionUpdatePayload`**: Sincroniza la posición 3D (`Vector3Dto`) junto con la marca de tiempo del cliente (`clientTimestamp`) para algoritmos de interpolación.

### 3. Mensajería y Chat (`ChatPackets.cs`)
- **`ChatSendMessagePayload`**: Entrada enviada desde el cliente con canal y texto.
- **`ChatBroadcastPayload`**: Salida difundida con id de mensaje, emisor, rol, canal y timestamp.
- **`ChatHistoryPayload`**: Colección con los últimos mensajes registrados para inicializar la ventana de chat.

### 4. Sistema y Errores (`SystemPackets.cs`)
- **`ErrorPayload`**: Estructura estándar para comunicar excepciones de protocolo o rechazos de validación.
- **`SystemAnnouncementPayload`**: Emisión de avisos globales del servidor a todos los clientes.
