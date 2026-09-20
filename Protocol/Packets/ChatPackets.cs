using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Protocol.Packets;

/// <summary>
/// Carga útil enviada por el cliente para transmitir un mensaje de chat hacia el servidor.
/// </summary>
public sealed class ChatSendMessagePayload
{
    /// <summary>
    /// Canal o sala destinataria (ej. "Global", "Team", "Whisper").
    /// </summary>
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = "Global";

    /// <summary>
    /// Contenido textual del mensaje.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Carga útil difundida por el servidor hacia los clientes con un mensaje de chat validado.
/// </summary>
public sealed class ChatBroadcastPayload
{
    /// <summary>
    /// Identificador único del mensaje emitido.
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del usuario emisor.
    /// </summary>
    [JsonPropertyName("senderId")]
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario del emisor.
    /// </summary>
    [JsonPropertyName("senderUsername")]
    public string SenderUsername { get; set; } = string.Empty;

    /// <summary>
    /// Rol del emisor para colorear o dar formato en la interfaz gráfica.
    /// </summary>
    [JsonPropertyName("senderRole")]
    public UserRole SenderRole { get; set; } = UserRole.Player;

    /// <summary>
    /// Canal al que pertenece el mensaje.
    /// </summary>
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = "Global";

    /// <summary>
    /// Contenido textual transmitido.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Marca de tiempo Unix en milisegundos en que el servidor procesó el mensaje.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}

/// <summary>
/// Carga útil que contiene una lista de mensajes históricos entregados al conectar.
/// </summary>
public sealed class ChatHistoryPayload
{
    /// <summary>
    /// Canal del cual procede el historial.
    /// </summary>
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = "Global";

    /// <summary>
    /// Lista de mensajes cronológicos previos.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ChatBroadcastPayload> Messages { get; set; } = new();
}
