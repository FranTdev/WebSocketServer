using System;

namespace WebSocketServer.Core.Models;

/// <summary>
/// Representa la entidad de dominio de un usuario registrado o conectado en el servidor de juego.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Identificador único universal (UUID) del usuario.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Nombre de usuario visible único en el sistema.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Rol y privilegios del usuario dentro del servidor.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Player;

    /// <summary>
    /// Última posición espacial conocida en el mundo virtual 3D.
    /// </summary>
    public Vector3Dto Position { get; set; } = Vector3Dto.Zero;

    /// <summary>
    /// Fecha y hora UTC en la que fue creada la cuenta o entidad.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora UTC de la última actividad o comunicación de red recibida.
    /// </summary>
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="User"/>.
    /// </summary>
    public User()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="User"/> con parámetros principales.
    /// </summary>
    /// <param name="id">Identificador único del usuario.</param>
    /// <param name="username">Nombre de usuario.</param>
    /// <param name="role">Rol asignado.</param>
    /// <param name="position">Posición espacial inicial.</param>
    public User(string id, string username, UserRole role, Vector3Dto position)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
        Username = username;
        Role = role;
        Position = position;
        CreatedAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza la posición espacial del usuario y renueva la marca temporal de última actividad.
    /// </summary>
    /// <param name="newPosition">Nueva coordenada en el espacio 3D.</param>
    public void UpdatePosition(Vector3Dto newPosition)
    {
        Position = newPosition;
        LastActiveAt = DateTime.UtcNow;
    }
}
