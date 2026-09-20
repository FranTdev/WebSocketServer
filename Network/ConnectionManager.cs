using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Interfaces;

namespace WebSocketServer.Network;

/// <summary>
/// Administrador central y concurrente de las sesiones de clientes conectadas al servidor.
/// </summary>
public sealed class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, IClientSession> _sessions = new();

    /// <summary>
    /// Evento emitido cuando una nueva sesión es agregada al administrador.
    /// </summary>
    public event Action<IClientSession>? SessionAdded;

    /// <summary>
    /// Evento emitido cuando una sesión es retirada y desconectada del servidor.
    /// </summary>
    public event Action<IClientSession>? SessionRemoved;

    /// <summary>
    /// Obtiene el número total de conexiones activas actualmente.
    /// </summary>
    public int ActiveConnectionsCount => _sessions.Count;

    /// <summary>
    /// Registra una nueva sesión en la colección interna de forma thread-safe.
    /// </summary>
    /// <param name="session">Instancia de la sesión de cliente.</param>
    public void AddSession(IClientSession session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));

        if (_sessions.TryAdd(session.SessionId, session))
        {
            SessionAdded?.Invoke(session);
        }
    }

    /// <summary>
    /// Retira una sesión de la colección interna y notifica su remoción.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión.</param>
    /// <returns><c>true</c> si fue removida; en caso contrario, <c>false</c>.</returns>
    public bool RemoveSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return false;

        if (_sessions.TryRemove(sessionId, out var session))
        {
            SessionRemoved?.Invoke(session);
            session.Dispose();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Intenta recuperar una sesión por su identificador único.
    /// </summary>
    /// <param name="sessionId">Identificador único.</param>
    /// <param name="session">Instancia encontrada o <c>null</c>.</param>
    /// <returns><c>true</c> si se encontró la sesión activa; de lo contrario, <c>false</c>.</returns>
    public bool TryGetSession(string sessionId, out IClientSession? session)
    {
        return _sessions.TryGetValue(sessionId, out session);
    }

    /// <summary>
    /// Obtiene una captura de solo lectura de todas las sesiones activas en este instante.
    /// </summary>
    /// <returns>Colección inmutable de sesiones.</returns>
    public IReadOnlyCollection<IClientSession> GetAllSessions()
    {
        return _sessions.Values as IReadOnlyCollection<IClientSession> ?? new List<IClientSession>(_sessions.Values);
    }

    /// <summary>
    /// Difunde un mensaje textual a todas las sesiones actualmente conectadas en paralelo sin bloquear.
    /// </summary>
    /// <param name="message">Mensaje serializado a emitir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la difusión asíncrona.</returns>
    public async Task BroadcastAsync(string message, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>(_sessions.Count);
        foreach (var pair in _sessions)
        {
            if (pair.Value.IsConnected)
            {
                tasks.Add(pair.Value.SendTextAsync(message, cancellationToken));
            }
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Difunde un mensaje a todas las sesiones excepto a la sesión emisora especificada.
    /// </summary>
    /// <param name="message">Mensaje serializado a emitir.</param>
    /// <param name="excludeSessionId">Identificador de la sesión que no debe recibir el mensaje.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la difusión selectiva.</returns>
    public async Task BroadcastExceptAsync(string message, string excludeSessionId, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>(_sessions.Count);
        foreach (var pair in _sessions)
        {
            if (pair.Key != excludeSessionId && pair.Value.IsConnected)
            {
                tasks.Add(pair.Value.SendTextAsync(message, cancellationToken));
            }
        }

        await Task.WhenAll(tasks);
    }
}
