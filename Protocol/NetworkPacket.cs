using System;
using System.Text.Json.Serialization;

namespace WebSocketServer.Protocol;

/// <summary>
/// Envoltorio estándar (Envelope) de los mensajes de red intercambiados entre cliente y servidor.
/// </summary>
public sealed class NetworkPacket
{
    /// <summary>
    /// Código de operación numérico que identifica el propósito y tipo de carga del paquete.
    /// </summary>
    [JsonPropertyName("opCode")]
    public short OpCode { get; set; }

    /// <summary>
    /// Carga útil del mensaje serializada en formato JSON.
    /// </summary>
    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Marca de tiempo Unix en milisegundos en el momento de creación del paquete.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NetworkPacket"/>.
    /// </summary>
    public NetworkPacket()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NetworkPacket"/> con el código y payload especificados.
    /// </summary>
    /// <param name="opCode">Código de operación.</param>
    /// <param name="payload">Contenido serializado.</param>
    public NetworkPacket(short opCode, string payload)
    {
        OpCode = opCode;
        Payload = payload;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
