# Módulo de Serialización de Red (`Protocol/Serialization`)

Este módulo proporciona la abstracción y la implementación del motor de serialización y deserialización JSON para el protocolo de red del servidor.

---

## Componentes

### 1. `INetworkSerializer.cs`
Interfaz de contrato que desacopla el formato de serialización de la lógica del protocolo. Permite intercambiar el serializador por MessagePack, Protobuf o BinaryPacker en el futuro sin modificar los manejadores de paquetes ni los servicios de dominio.

### 2. `JsonNetworkSerializer.cs`
Implementación basada en `System.Text.Json` configurada con opciones de alto rendimiento:
- `PropertyNameCaseInsensitive = true`: Tolerancia a variaciones de mayúsculas/minúsculas entre clientes.
- `WriteIndented = false`: Generación de JSON compacto y de longitud mínima para no desperdiciar ancho de banda.
- `DefaultIgnoreCondition = WhenWritingNull`: Omisión de campos nulos en las respuestas para reducir el tamaño de las tramas en red.
- `Instance`: Propiedad estática reutilizable y thread-safe para evitar asignaciones continuas en el recolector de basura (*Garbage Collector*).
