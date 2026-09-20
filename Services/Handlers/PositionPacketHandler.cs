using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Interfaces;
using WebSocketServer.Protocol;
using WebSocketServer.Protocol.Packets;
using WebSocketServer.Protocol.Serialization;

namespace WebSocketServer.Services.Handlers;

/// <summary>
/// Manejador del paquete de sincronización de movimiento (<see cref="OpCodes.PositionUpdate"/>).
/// Actualiza las coordenadas del usuario y las retransmite a los demás jugadores conectados.
/// </summary>
public sealed class PositionPacketHandler : IPacketHandler
{
    private readonly UserService _userService;
    private readonly IConnectionManager _connectionManager;
    private readonly INetworkSerializer _serializer;

    /// <summary>
    /// Código de operación asignado (<see cref="OpCodes.PositionUpdate"/>).
    /// </summary>
    public short OpCode => OpCodes.PositionUpdate;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PositionPacketHandler"/>.
    /// </summary>
    /// <param name="userService">Servicio de usuario.</param>
    /// <param name="connectionManager">Administrador de sesiones activas.</param>
    /// <param name="serializer">Serializador del protocolo.</param>
    public PositionPacketHandler(UserService userService, IConnectionManager connectionManager, INetworkSerializer serializer)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Procesa el paquete de actualización de posición espacial de un usuario autenticado.
    /// </summary>
    /// <param name="session">Sesión del cliente.</param>
    /// <param name="payloadJson">Cuerpo JSON con el DTO <see cref="PositionUpdatePayload"/>.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa el procesamiento y la retransmisión.</returns>
    public async Task HandleAsync(IClientSession session, string payloadJson, CancellationToken cancellationToken = default)
    {
        var user = session.AuthenticatedUser;
        if (user == null)
        {
            // Rechazar paquetes de movimiento si la sesión no ha sido autenticada
            var error = new ErrorPayload("UNAUTHORIZED", "Debe iniciar sesión antes de enviar actualizaciones de posición.");
            var errorPacket = new NetworkPacket(OpCodes.ErrorNotification, _serializer.Serialize(error));
            await session.SendTextAsync(_serializer.Serialize(errorPacket), cancellationToken);
            return;
        }

        var updatePayload = _serializer.Deserialize<PositionUpdatePayload>(payloadJson);
        if (updatePayload == null)
        {
            return;
        }

        // Actualizar el estado en memoria de la sesión
        user.UpdatePosition(updatePayload.Position);

        // Actualizar la base de datos de manera desacoplada
        await _userService.UpdateUserPositionAsync(user.Id, updatePayload.Position, cancellationToken);

        // Retransmitir la posición a todos los demás clientes (excluyendo al remitente para evitar eco)
        var broadcastUpdate = new PositionUpdatePayload
        {
            UserId = user.Id,
            Position = updatePayload.Position,
            ClientTimestamp = updatePayload.ClientTimestamp
        };

        var packet = new NetworkPacket(OpCodes.PositionUpdate, _serializer.Serialize(broadcastUpdate));
        await _connectionManager.BroadcastExceptAsync(_serializer.Serialize(packet), session.SessionId, cancellationToken);
    }
}
