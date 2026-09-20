using System;
using System.Collections.Generic;
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
/// Servicio de dominio responsable de la autenticación, gestión de cuentas y sincronización de usuarios en línea.
/// </summary>
public sealed class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConnectionManager _connectionManager;
    private readonly INetworkSerializer _serializer;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UserService"/>.
    /// </summary>
    /// <param name="userRepository">Repositorio de persistencia de usuarios.</param>
    /// <param name="connectionManager">Administrador de conexiones activas.</param>
    /// <param name="serializer">Serializador del protocolo de red.</param>
    public UserService(IUserRepository userRepository, IConnectionManager connectionManager, INetworkSerializer serializer)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Autentica a un usuario existente o crea uno nuevo de forma persistente si el nombre de usuario no existe.
    /// </summary>
    /// <param name="username">Nombre de usuario deseado.</param>
    /// <param name="desiredRole">Rol propuesto para la cuenta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Entidad de usuario autenticada o creada.</returns>
    public async Task<User> AuthenticateOrRegisterAsync(string username, UserRole desiredRole, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(username));
        }

        string cleanUsername = username.Trim();
        User? user = await _userRepository.GetByUsernameAsync(cleanUsername, cancellationToken);

        if (user == null)
        {
            // Registrar nuevo usuario
            user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = cleanUsername,
                Role = desiredRole == UserRole.Admin ? UserRole.Player : desiredRole, // Evitar autoproclamación de Admin arbitrario
                Position = new Vector3Dto(0f, 1f, 0f),
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };

            await _userRepository.UpsertAsync(user, cancellationToken);
            Console.WriteLine($"[UserService] Nuevo usuario registrado y persistido: {user.Username} ({user.Id})");
        }
        else
        {
            user.LastActiveAt = DateTime.UtcNow;
            await _userRepository.UpsertAsync(user, cancellationToken);
            Console.WriteLine($"[UserService] Usuario autenticado: {user.Username} ({user.Id})");
        }

        return user;
    }

    /// <summary>
    /// Emite la lista de jugadores conectados al usuario recién autenticado y notifica a los demás de su ingreso.
    /// </summary>
    /// <param name="session">Sesión del cliente autenticado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la sincronización de presencia.</returns>
    public async Task NotifyUserJoinedAsync(IClientSession session, CancellationToken cancellationToken = default)
    {
        var authenticatedUser = session.AuthenticatedUser;
        if (authenticatedUser == null) return;

        // 1. Enviar la lista de todos los usuarios ya presentes en el servidor al nuevo jugador
        var onlineUsers = _connectionManager.GetAllSessions()
            .Where(s => s.AuthenticatedUser != null && s.SessionId != session.SessionId)
            .Select(s => new UserSyncPayload
            {
                UserId = s.AuthenticatedUser!.Id,
                Username = s.AuthenticatedUser.Username,
                Role = s.AuthenticatedUser.Role,
                Position = s.AuthenticatedUser.Position
            })
            .ToList();

        var syncPacket = new NetworkPacket(OpCodes.UserListSync, _serializer.Serialize(onlineUsers));
        await session.SendTextAsync(_serializer.Serialize(syncPacket), cancellationToken);

        // 2. Difundir a todos los demás jugadores que un nuevo usuario ha ingresado
        var joinedPayload = new UserSyncPayload
        {
            UserId = authenticatedUser.Id,
            Username = authenticatedUser.Username,
            Role = authenticatedUser.Role,
            Position = authenticatedUser.Position
        };

        var broadcastPacket = new NetworkPacket(OpCodes.UserJoined, _serializer.Serialize(joinedPayload));
        await _connectionManager.BroadcastExceptAsync(_serializer.Serialize(broadcastPacket), session.SessionId, cancellationToken);
    }

    /// <summary>
    /// Notifica a los demás clientes conectados que un usuario ha abandonado la sesión.
    /// </summary>
    /// <param name="session">Sesión desconectada.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea asíncrona.</returns>
    public async Task NotifyUserLeftAsync(IClientSession session, CancellationToken cancellationToken = default)
    {
        var user = session.AuthenticatedUser;
        if (user == null) return;

        var leftPayload = new UserSyncPayload
        {
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            Position = user.Position
        };

        var packet = new NetworkPacket(OpCodes.UserLeft, _serializer.Serialize(leftPayload));
        await _connectionManager.BroadcastAsync(_serializer.Serialize(packet), cancellationToken);
    }

    /// <summary>
    /// Actualiza la posición espacial del usuario en memoria y en la base de datos de manera eficiente.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="newPosition">Nueva coordenada espacial.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea asíncrona.</returns>
    public async Task UpdateUserPositionAsync(string userId, Vector3Dto newPosition, CancellationToken cancellationToken = default)
    {
        await _userRepository.UpdatePositionAsync(userId, newPosition, cancellationToken);
    }
}
