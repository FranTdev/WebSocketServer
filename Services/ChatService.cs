using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Interfaces;
using WebSocketServer.Core.Models;
using WebSocketServer.Protocol;
using WebSocketServer.Protocol.Packets;
using WebSocketServer.Protocol.Serialization;

namespace WebSocketServer.Services;

/// <summary>
/// Servicio de dominio para el procesamiento, filtrado, persistencia y difusión de mensajes de chat en tiempo real.
/// </summary>
public sealed class ChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IConnectionManager _connectionManager;
    private readonly INetworkSerializer _serializer;

    /// <summary>
    /// Longitud máxima en caracteres permitida para un mensaje de chat.
    /// </summary>
    public const int MaxMessageLength = 500;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ChatService"/>.
    /// </summary>
    /// <param name="chatRepository">Repositorio de persistencia de chat.</param>
    /// <param name="connectionManager">Administrador de conexiones activas.</param>
    /// <param name="serializer">Serializador del protocolo de red.</param>
    public ChatService(IChatRepository chatRepository, IConnectionManager connectionManager, INetworkSerializer serializer)
    {
        _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Procesa el mensaje emitido por un usuario, lo persiste en la base de datos y lo difunde a todos los clientes.
    /// </summary>
    /// <param name="sender">Usuario emisor autenticado.</param>
    /// <param name="channel">Canal o sala de destino.</param>
    /// <param name="content">Texto del mensaje.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa el procesamiento y la difusión.</returns>
    public async Task ProcessAndBroadcastMessageAsync(User sender, string channel, string content, CancellationToken cancellationToken = default)
    {
        if (sender == null) throw new ArgumentNullException(nameof(sender));

        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        string sanitizedContent = content.Trim();
        if (sanitizedContent.Length > MaxMessageLength)
        {
            sanitizedContent = sanitizedContent.Substring(0, MaxMessageLength);
        }

        string cleanChannel = string.IsNullOrWhiteSpace(channel) ? "Global" : channel.Trim();

        var chatMessage = new ChatMessage(sender.Id, sender.Username, sanitizedContent, cleanChannel);

        // 1. Persistir en la base de datos ultraligera SQLite
        await _chatRepository.SaveMessageAsync(chatMessage, cancellationToken);

        // 2. Difundir a todos los clientes conectados
        var broadcastPayload = new ChatBroadcastPayload
        {
            MessageId = chatMessage.Id,
            SenderId = sender.Id,
            SenderUsername = sender.Username,
            SenderRole = sender.Role,
            Channel = cleanChannel,
            Content = sanitizedContent,
            Timestamp = chatMessage.TimestampUnixMilliseconds
        };

        var packet = new NetworkPacket(OpCodes.ChatBroadcast, _serializer.Serialize(broadcastPayload));
        await _connectionManager.BroadcastAsync(_serializer.Serialize(packet), cancellationToken);

        Console.WriteLine($"[ChatService] [{cleanChannel}] <{sender.Username}>: {sanitizedContent}");
    }

    /// <summary>
    /// Envía el historial de mensajes recientes al usuario que acaba de conectarse.
    /// </summary>
    /// <param name="session">Sesión del cliente destinatario.</param>
    /// <param name="channel">Canal a consultar.</param>
    /// <param name="limit">Cantidad máxima de mensajes a enviar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea asíncrona.</returns>
    public async Task SendChatHistoryAsync(IClientSession session, string channel = "Global", int limit = 30, CancellationToken cancellationToken = default)
    {
        var messages = await _chatRepository.GetRecentMessagesAsync(channel, limit, cancellationToken);

        var historyPayload = new ChatHistoryPayload
        {
            Channel = channel,
            Messages = messages.Select(m => new ChatBroadcastPayload
            {
                MessageId = m.Id,
                SenderId = m.SenderId,
                SenderUsername = m.SenderUsername,
                SenderRole = UserRole.Player,
                Channel = m.Channel,
                Content = m.Content,
                Timestamp = m.TimestampUnixMilliseconds
            }).ToList()
        };

        var packet = new NetworkPacket(OpCodes.ChatHistory, _serializer.Serialize(historyPayload));
        await session.SendTextAsync(_serializer.Serialize(packet), cancellationToken);
    }
}
