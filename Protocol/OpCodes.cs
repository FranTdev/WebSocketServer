namespace WebSocketServer.Protocol;

/// <summary>
/// Catálogo central de códigos de operación numéricos (OpCodes) para el protocolo de red.
/// Los códigos están agrupados por rangos funcionales para maximizar la legibilidad y rendimiento.
/// </summary>
public static class OpCodes
{
    // ==========================================
    // Rango 0 - 99: Conexión, Latido y Handshake
    // ==========================================

    /// <summary>
    /// Paquete de verificación de latido o comprobación de latencia (Ping).
    /// </summary>
    public const short Ping = 1;

    /// <summary>
    /// Respuesta a la verificación de latido (Pong).
    /// </summary>
    public const short Pong = 2;

    // ==========================================
    // Rango 100 - 199: Autenticación y Cuentas
    // ==========================================

    /// <summary>
    /// Solicitud de inicio de sesión o registro emitida por el cliente.
    /// </summary>
    public const short AuthRequest = 100;

    /// <summary>
    /// Respuesta del servidor confirmando o rechazando la autenticación del usuario.
    /// </summary>
    public const short AuthResponse = 101;

    // ==========================================
    // Rango 200 - 299: Estado de Jugadores y Presencia
    // ==========================================

    /// <summary>
    /// Notificación emitida por el servidor cuando un nuevo jugador ingresa al mundo.
    /// </summary>
    public const short UserJoined = 200;

    /// <summary>
    /// Notificación emitida por el servidor cuando un jugador se desconecta del mundo.
    /// </summary>
    public const short UserLeft = 201;

    /// <summary>
    /// Sincronización inicial del listado de jugadores presentes al momento de conectarse.
    /// </summary>
    public const short UserListSync = 202;

    // ==========================================
    // Rango 300 - 399: Movimiento y Posicionamiento Espacial
    // ==========================================

    /// <summary>
    /// Paquete de actualización de posición 3D (enviado por cliente y retransmitido por el servidor).
    /// </summary>
    public const short PositionUpdate = 300;

    // ==========================================
    // Rango 400 - 499: Sistema de Chat y Comunicación
    // ==========================================

    /// <summary>
    /// Envío de un mensaje de chat desde el cliente hacia el servidor.
    /// </summary>
    public const short ChatSendMessage = 400;

    /// <summary>
    /// Difusión de un mensaje de chat desde el servidor hacia los clientes conectados.
    /// </summary>
    public const short ChatBroadcast = 401;

    /// <summary>
    /// Entrega del historial reciente de mensajes de chat al iniciar sesión.
    /// </summary>
    public const short ChatHistory = 402;

    // ==========================================
    // Rango 500 - 599: Mensajes del Sistema y Errores
    // ==========================================

    /// <summary>
    /// Paquete de notificación de error con código y mensaje descriptivo.
    /// </summary>
    public const short ErrorNotification = 500;

    /// <summary>
    /// Notificación global del sistema o anuncio del servidor.
    /// </summary>
    public const short SystemAnnouncement = 501;
}
