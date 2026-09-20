using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Interfaces;
using WebSocketServer.Protocol;
using WebSocketServer.Protocol.Packets;
using WebSocketServer.Protocol.Serialization;

namespace WebSocketServer.Network;

/// <summary>
/// Enrutador central que deserializa los paquetes entrantes y delega su procesamiento
/// al manejador específico (<see cref="IPacketHandler"/>) según el código de operación.
/// </summary>
public sealed class PacketRouter
{
    private readonly ConcurrentDictionary<short, IPacketHandler> _handlers = new();
    private readonly INetworkSerializer _serializer;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PacketRouter"/>.
    /// </summary>
    /// <param name="serializer">Serializador para decodificar sobres de red.</param>
    public PacketRouter(INetworkSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Registra un manejador de paquete para su respectivo <see cref="IPacketHandler.OpCode"/>.
    /// </summary>
    /// <param name="handler">Instancia del manejador a asociar.</param>
    public void RegisterHandler(IPacketHandler handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        _handlers[handler.OpCode] = handler;
    }

    /// <summary>
    /// Procesa una trama de texto entrante recibida de un cliente, deserializa su sobre y despacha al manejador correspondiente.
    /// </summary>
    /// <param name="session">Sesión del cliente remitente.</param>
    /// <param name="rawText">Contenido textual del paquete recibido.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa el flujo de enrutamiento y procesamiento.</returns>
    public async Task RoutePacketAsync(IClientSession session, string rawText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return;
        }

        NetworkPacket? packet;
        try
        {
            packet = _serializer.Deserialize<NetworkPacket>(rawText);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[PacketRouter] Error deserializando sobre de red de sesión {session.SessionId}: {ex.Message}");
            Console.ResetColor();

            var errorPacket = new NetworkPacket(OpCodes.ErrorNotification, _serializer.Serialize(new ErrorPayload("MALFORMED_PACKET", "El sobre de red es inválido.")));
            await session.SendTextAsync(_serializer.Serialize(errorPacket), cancellationToken);
            return;
        }

        if (packet == null)
        {
            return;
        }

        // Manejo nativo de latencia Ping / Pong
        if (packet.OpCode == OpCodes.Ping)
        {
            var pongPacket = new NetworkPacket(OpCodes.Pong, string.Empty);
            await session.SendTextAsync(_serializer.Serialize(pongPacket), cancellationToken);
            return;
        }

        if (_handlers.TryGetValue(packet.OpCode, out var handler))
        {
            try
            {
                await handler.HandleAsync(session, packet.Payload, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[PacketRouter] Excepción al procesar OpCode {packet.OpCode} para sesión {session.SessionId}: {ex.Message}");
                Console.ResetColor();

                var errorPacket = new NetworkPacket(OpCodes.ErrorNotification, _serializer.Serialize(new ErrorPayload("INTERNAL_SERVER_ERROR", "Error interno al procesar la solicitud.")));
                await session.SendTextAsync(_serializer.Serialize(errorPacket), cancellationToken);
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[PacketRouter] No hay manejador registrado para el OpCode {packet.OpCode}.");
            Console.ResetColor();

            var errorPacket = new NetworkPacket(OpCodes.ErrorNotification, _serializer.Serialize(new ErrorPayload("UNKNOWN_OPCODE", $"El código de operación {packet.OpCode} no está reconocido por el servidor.")));
            await session.SendTextAsync(_serializer.Serialize(errorPacket), cancellationToken);
        }
    }
}
