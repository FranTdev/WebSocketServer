using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Interfaces;
using WebSocketServer.Protocol;
using WebSocketServer.Protocol.Packets;
using WebSocketServer.Protocol.Serialization;

namespace WebSocketServer.Services.Handlers;

/// <summary>
/// Manejador del paquete de autenticación (<see cref="OpCodes.AuthRequest"/>).
/// Valida las credenciales de entrada, asocia el usuario a la sesión y difunde la presencia.
/// </summary>
public sealed class AuthPacketHandler : IPacketHandler
{
    private readonly UserService _userService;
    private readonly ChatService _chatService;
    private readonly INetworkSerializer _serializer;

    /// <summary>
    /// Código de operación numérico asignado (<see cref="OpCodes.AuthRequest"/>).
    /// </summary>
    public short OpCode => OpCodes.AuthRequest;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AuthPacketHandler"/>.
    /// </summary>
    /// <param name="userService">Servicio de usuario.</param>
    /// <param name="chatService">Servicio de chat.</param>
    /// <param name="serializer">Serializador del protocolo.</param>
    public AuthPacketHandler(UserService userService, ChatService chatService, INetworkSerializer serializer)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Procesa la solicitud de autenticación recibida desde un cliente WebSocket.
    /// </summary>
    /// <param name="session">Sesión del cliente.</param>
    /// <param name="payloadJson">Cuerpo JSON con el DTO <see cref="AuthRequestPayload"/>.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa el flujo de autenticación.</returns>
    public async Task HandleAsync(IClientSession session, string payloadJson, CancellationToken cancellationToken = default)
    {
        var request = _serializer.Deserialize<AuthRequestPayload>(payloadJson);
        if (request == null || string.IsNullOrWhiteSpace(request.Username))
        {
            var failResponse = new AuthResponsePayload
            {
                Success = false,
                ErrorMessage = "El nombre de usuario proporcionado no es válido."
            };
            var failPacket = new NetworkPacket(OpCodes.AuthResponse, _serializer.Serialize(failResponse));
            await session.SendTextAsync(_serializer.Serialize(failPacket), cancellationToken);
            return;
        }

        try
        {
            var user = await _userService.AuthenticateOrRegisterAsync(request.Username, request.Role, cancellationToken);
            session.AssociateUser(user);

            var successResponse = new AuthResponsePayload
            {
                Success = true,
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role,
                SpawnPosition = user.Position
            };

            var successPacket = new NetworkPacket(OpCodes.AuthResponse, _serializer.Serialize(successResponse));
            await session.SendTextAsync(_serializer.Serialize(successPacket), cancellationToken);

            // Sincronizar jugadores presentes en el servidor y notificar a los demás
            await _userService.NotifyUserJoinedAsync(session, cancellationToken);

            // Enviar los últimos mensajes del historial de chat
            await _chatService.SendChatHistoryAsync(session, "Global", 30, cancellationToken);
        }
        catch (Exception ex)
        {
            var errorResponse = new AuthResponsePayload
            {
                Success = false,
                ErrorMessage = $"Error en la autenticación: {ex.Message}"
            };
            var errorPacket = new NetworkPacket(OpCodes.AuthResponse, _serializer.Serialize(errorResponse));
            await session.SendTextAsync(_serializer.Serialize(errorPacket), cancellationToken);
        }
    }
}
