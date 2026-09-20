using System;

namespace WebSocketServer.Core.Models;

/// <summary>
/// Representa la entidad de un mensaje de chat transmitido y persistido en el servidor.
/// </summary>
public sealed class ChatMessage
{
    /// <summary>
    /// Identificador único del mensaje generado por el servidor.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identificador del usuario emisor del mensaje.
    /// </summary>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario visible del emisor al momento del envío.
    /// </summary>
    public string SenderUsername { get; set; } = string.Empty;

    /// <summary>
    /// Contenido textual del mensaje.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Canal o ámbito del mensaje (ej. "Global", "System", "Whisper").
    /// </summary>
    public string Channel { get; set; } = "Global";

    /// <summary>
    /// Marca temporal en milisegundos Unix UTC en que fue recibido y procesado el mensaje.
    /// </summary>
    public long TimestampUnixMilliseconds { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ChatMessage"/>.
    /// </summary>
    public ChatMessage()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ChatMessage"/> con los datos del emisor y texto.
    /// </summary>
    /// <param name="senderId">Identificador del usuario emisor.</param>
    /// <param name="senderUsername">Nombre público del usuario emisor.</param>
    /// <param name="content">Texto del mensaje.</param>
    /// <param name="channel">Canal o sala de destino.</param>
    public ChatMessage(string senderId, string senderUsername, string content, string channel = "Global")
    {
        Id = Guid.NewGuid().ToString();
        SenderId = senderId;
        SenderUsername = senderUsername;
        Content = content;
        Channel = channel;
        TimestampUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
