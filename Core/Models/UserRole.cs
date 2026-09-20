namespace WebSocketServer.Core.Models;

/// <summary>
/// Define los roles y niveles de autorización de los usuarios en el sistema.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Usuario no autenticado o con permisos mínimos de sólo lectura.
    /// </summary>
    Guest = 0,

    /// <summary>
    /// Jugador estándar con capacidad de interactuar, moverse y enviar mensajes de chat.
    /// </summary>
    Player = 1,

    /// <summary>
    /// Moderador con facultades de silenciar usuarios y supervisar canales.
    /// </summary>
    Moderator = 2,

    /// <summary>
    /// Administrador con control total sobre el servidor, configuración y entidades.
    /// </summary>
    Admin = 3
}
