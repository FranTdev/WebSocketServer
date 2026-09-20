using System;
using System.Text.Json.Serialization;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Protocol.Packets;

/// <summary>
/// Carga útil para la sincronización periódica de la posición y rotación espacial de un jugador.
/// </summary>
public sealed class PositionUpdatePayload
{
    /// <summary>
    /// Identificador del usuario cuya posición se actualiza.
    /// Puede omitirse por el cliente ya que el servidor asocia la sesión autenticada.
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Posición en coordenadas espaciales 3D en el mundo virtual.
    /// </summary>
    [JsonPropertyName("position")]
    public Vector3Dto Position { get; set; } = Vector3Dto.Zero;

    /// <summary>
    /// Marca temporal en milisegundos en la que se generó la muestra de posición en el cliente.
    /// </summary>
    [JsonPropertyName("clientTimestamp")]
    public long ClientTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
