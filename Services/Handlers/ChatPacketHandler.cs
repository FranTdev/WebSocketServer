using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Interfaces;
using WebSocketServer.Protocol;
using WebSocketServer.Protocol.Packets;
using WebSocketServer.Protocol.Serialization;

namespace WebSocketServer.Services.Handlers;

/// <summary>
/// Manejador de paquetes de mensajería de chat (<see cref="OpCodes.ChatSendMessage"/>).
/// Valida la sesión del emisor y canaliza el mensaje a través de <see cref="ChatService"/>.
/// </summary>
public sealed class ChatPacketHandler : IPacketHandler
{
    private readonly ChatService _chatService;
    private readonly INetworkSerializer _serializer;

    /// <summary>
    /// Código de operación numérico asignado (<see cref="OpCodes.ChatSendMessage"/>).
    /// </summary>
    public short OpCode => OpCodes.ChatSendMessage;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ChatPacketHandler"/>.
    /// </summary>
    /// <param name="chatService">Servicio de dominio de chat.</param>
    /// <param name="serializer">Serializador del protocolo.</param>
    public ChatPacketHandler(ChatService chatService, INetworkSerializer serializer)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Procesa el mensaje de chat remitido por el cliente.
    /// </summary>
    /// <param name="session">Sesión del cliente remitente.</param>
    /// <param name="payloadJson">Cuerpo JSON con el DTO <see cref="ChatSendMessagePayload"/>.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa el procesamiento del chat.</returns>
    public async Task HandleAsync(IClientSession session, string payloadJson, CancellationToken cancellationToken = default)
    {
        var user = session.AuthenticatedUser;
        if (user == null)
        {
            var error = new ErrorPayload("UNAUTHORIZED", "Debe identificarse antes de participar en el chat.");
            var errorPacket = new NetworkPacket(OpCodes.ErrorNotification, _serializer.Serialize(error));
            await session.SendTextAsync(_serializer.Serialize(errorPacket), cancellationToken);
            return;
        }

        var messagePayload = _serializer.Deserialize<ChatSendMessagePayload>(payloadJson);
        if (messagePayload == null || string.IsNullOrWhiteSpace(messagePayload.Content))
        {
            return;
        }

        await _chatService.ProcessAndBroadcastMessageAsync(
            user, 
            messagePayload.Channel, 
            messagePayload.Content, 
            cancellationToken
        );
    }
}
