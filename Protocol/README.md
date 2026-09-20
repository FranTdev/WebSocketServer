# Capa de Protocolo de Red (`Protocol`)

El módulo `Protocol` define las reglas de comunicación binaria y textual entre clientes (Unity u otros) y el servidor .NET. Establece un formato de paquete en sobre (*Envelope*) que encapsula el código de operación, la carga útil y metadatos de sincronización temporal.

---

## Formato del Paquete (`NetworkPacket`)

Todos los mensajes transmitidos por el WebSocket siguen la siguiente estructura JSON en sobre:

```json
{
  "opCode": 100,
  "payload": "{\"username\":\"PlayerOne\",\"role\":1}",
  "timestamp": 1726798800000
}
```

- **`opCode`** (`short`): Identifica la acción requerida.
- **`payload`** (`string`): Cadena JSON que contiene el DTO específico definido en `Protocol/Packets/`.
- **`timestamp`** (`long`): Milisegundos Unix para cálculo de latencia (*RTT*), ordenamiento e interpolación.

---

## Módulos y Directorios

```
Protocol/
├── OpCodes.cs                  # Catálogo de códigos de operación numéricos
├── NetworkPacket.cs            # Estructura del envoltorio de red
├── Packets/                    # Cargas útiles específicas (Auth, Position, Chat, System)
│   └── README.md
├── Serialization/              # Interfaces y serializador JSON de alta velocidad
└── README.md
```

---

## Flujo de Procesamiento

1. **Lectura**: El socket recibe una trama UTF-8 de texto.
2. **Desenvoltura**: `JsonNetworkSerializer` deserializa a `NetworkPacket`.
3. **Enrutamiento**: El `PacketRouter` localiza el `IPacketHandler` suscrito al `OpCode`.
4. **Desempaquetado**: El manejador deserializa el campo `payload` al DTO concreto y ejecuta la lógica de negocio.
