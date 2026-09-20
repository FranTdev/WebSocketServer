namespace WebSocketServer.Configuration;

/// <summary>
/// Proporciona la configuración global y parámetros operativos del servidor WebSocket.
/// </summary>
public class ServerConfig
{
    /// <summary>
    /// Obtiene o establece la dirección URL y prefijo en el que escuchará el listener HTTP/WebSocket.
    /// Soporta variables de entorno LISTENER_PREFIX y PORT.
    /// Por defecto escucha en http://localhost:8080/.
    /// </summary>
    public string ListenerPrefix { get; set; } =
        Environment.GetEnvironmentVariable("LISTENER_PREFIX")
        ?? (Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } port
            ? $"http://*:{port}/"
            : "http://localhost:8080/");

    /// <summary>
    /// Obtiene o establece el tamaño del búfer de recepción en bytes asignado a cada sesión WebSocket.
    /// Por defecto 4096 bytes (4 KB).
    /// </summary>
    public int ReceiveBufferSize { get; set; } =
        int.TryParse(Environment.GetEnvironmentVariable("RECEIVE_BUFFER_SIZE"), out int recvSize) ? recvSize : 4096;

    /// <summary>
    /// Obtiene o establece el tamaño del búfer de envío en bytes para la serialización de mensajes de salida.
    /// </summary>
    public int SendBufferSize { get; set; } =
        int.TryParse(Environment.GetEnvironmentVariable("SEND_BUFFER_SIZE"), out int sendSize) ? sendSize : 4096;

    /// <summary>
    /// Obtiene o establece el intervalo de tiempo en segundos para el latido (Keep-Alive / Heartbeat) de los WebSockets.
    /// </summary>
    public int KeepAliveIntervalSeconds { get; set; } =
        int.TryParse(Environment.GetEnvironmentVariable("KEEP_ALIVE_SECONDS"), out int keepAlive) ? keepAlive : 30;

    /// <summary>
    /// Obtiene o establece la cadena de conexión para la base de datos SQLite embebida.
    /// Soporta variables de entorno DATABASE_CONNECTION_STRING y DATABASE_PATH.
    /// </summary>
    public string DatabaseConnectionString { get; set; } =
        Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
        ?? (Environment.GetEnvironmentVariable("DATABASE_PATH") is { Length: > 0 } dbPath
            ? $"Data Source={dbPath};Cache=Shared;"
            : "Data Source=GameServerData.db;Cache=Shared;");

    /// <summary>
    /// Obtiene o establece el límite máximo de conexiones concurrentes permitidas en el servidor.
    /// </summary>
    public int MaxConcurrentConnections { get; set; } =
        int.TryParse(Environment.GetEnvironmentVariable("MAX_CONCURRENT_CONNECTIONS"), out int maxConn) ? maxConn : 1000;
}
