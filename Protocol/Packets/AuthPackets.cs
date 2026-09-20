using System.Text.Json.Serialization;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Protocol.Packets;

/// <summary>
/// Carga útil del paquete de solicitud de autenticación enviado por el cliente.
/// </summary>
public sealed class AuthRequestPayload
{
    /// <summary>
    /// Nombre de usuario solicitado para el inicio de sesión.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Rol deseado en caso de registro o solicitud especial (por defecto Player).
    /// </summary>
    [JsonPropertyName("role")]
    public UserRole Role { get; set; } = UserRole.Player;
}

/// <summary>
/// Carga útil del paquete de respuesta de autenticación emitida por el servidor.
/// </summary>
public sealed class AuthResponsePayload
{
    /// <summary>
    /// Indica si el proceso de autenticación fue exitoso.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Identificador único asignado al usuario.
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario confirmado.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Rol y privilegios concedidos en el servidor.
    /// </summary>
    [JsonPropertyName("role")]
    public UserRole Role { get; set; } = UserRole.Player;

    /// <summary>
    /// Posición espacial inicial donde aparece el usuario en el mundo 3D.
    /// </summary>
    [JsonPropertyName("spawnPosition")]
    public Vector3Dto SpawnPosition { get; set; } = Vector3Dto.Zero;

    /// <summary>
    /// Mensaje descriptivo de error en caso de fallo; de lo contrario, <c>null</c> o vacío.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Carga útil para sincronizar el estado de un usuario que se une o ya se encuentra en el servidor.
/// </summary>
public sealed class UserSyncPayload
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario visible.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Rol asignado al usuario.
    /// </summary>
    [JsonPropertyName("role")]
    public UserRole Role { get; set; } = UserRole.Player;

    /// <summary>
    /// Posición tridimensional actual.
    /// </summary>
    [JsonPropertyName("position")]
    public Vector3Dto Position { get; set; } = Vector3Dto.Zero;
}
