using System;
using System.Text.Json.Serialization;

namespace WebSocketServer.Protocol.Packets;

/// <summary>
/// Carga útil utilizada para notificar errores controlados al cliente.
/// </summary>
public sealed class ErrorPayload
{
    /// <summary>
    /// Código identificador del error (ej. "AUTH_FAILED", "RATE_LIMITED", "INVALID_PACKET").
    /// </summary>
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje descriptivo del error comprensible por el usuario o para depuración.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ErrorPayload"/>.
    /// </summary>
    public ErrorPayload()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ErrorPayload"/> con código y mensaje.
    /// </summary>
    /// <param name="errorCode">Código del error.</param>
    /// <param name="message">Descripción del error.</param>
    public ErrorPayload(string errorCode, string message)
    {
        ErrorCode = errorCode;
        Message = message;
    }
}

/// <summary>
/// Carga útil para emitir anuncios administrativos o mensajes del sistema en el servidor.
/// </summary>
public sealed class SystemAnnouncementPayload
{
    /// <summary>
    /// Texto del anuncio emitido por el sistema.
    /// </summary>
    [JsonPropertyName("announcement")]
    public string Announcement { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de severidad o urgencia (ej. "Info", "Warning", "Shutdown").
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "Info";

    /// <summary>
    /// Marca temporal de emisión del anuncio.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
