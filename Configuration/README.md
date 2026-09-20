# Módulo de Configuración (`Configuration`)

El módulo `Configuration` centraliza los parámetros operativos, puertos de red, parámetros de rendimiento de WebSockets y cadenas de conexión a la base de datos para el servidor.

---

## Componentes

### 1. `ServerConfig.cs`
Clase de configuración fuertemente tipada que define las propiedades esenciales del servidor:
- **`ListenerPrefix`**: Prefijo URL HTTP/WS en el que se reciben conexiones (ej. `http://localhost:8080/` o `http://*:8080/`).
- **`ReceiveBufferSize`**: Tamaño en bytes del búfer de lectura por sesión (por defecto `4096` bytes).
- **`SendBufferSize`**: Tamaño en bytes para tramas de transmisión salientes.
- **`KeepAliveIntervalSeconds`**: Frecuencia del ping/pong nativo de WebSockets para detectar desconexiones silenciosas.
- **`DatabaseConnectionString`**: Cadena de conexión a la base de datos SQLite embebida optimizada con `Cache=Shared`.
- **`MaxConcurrentConnections`**: Capacidad máxima de conexiones simultáneas para protección contra saturación.

---

## Uso y Personalización

Para instanciar o cargar una configuración personalizada:

```csharp
var config = new ServerConfig
{
    ListenerPrefix = "http://0.0.0.0:8080/",
    ReceiveBufferSize = 8192,
    DatabaseConnectionString = "Data Source=ProductionServer.db;Cache=Shared;"
};
```
