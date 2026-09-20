using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketServer.Core.Interfaces;

/// <summary>
/// Gestiona el registro, rastreo y difusión de mensajes entre las sesiones activas en el servidor.
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// Registra una nueva sesión activa en el catálogo del servidor.
    /// </summary>
    /// <param name="session">Instancia de la sesión de cliente.</param>
    void AddSession(IClientSession session);

    /// <summary>
    /// Da de baja y retira una sesión activa cuando se desconecta.
    /// </summary>
    /// <param name="sessionId">Identificador de la sesión a remover.</param>
    /// <returns><c>true</c> si fue removida exitosamente; <c>false</c> si no existía.</returns>
    bool RemoveSession(string sessionId);

    /// <summary>
    /// Intenta recuperar una sesión activa por su identificador.
    /// </summary>
    /// <param name="sessionId">Identificador de la sesión.</param>
    /// <param name="session">Instancia de la sesión encontrada o <c>null</c>.</param>
    /// <returns><c>true</c> si la sesión existe y está activa; de lo contrario, <c>false</c>.</returns>
    bool TryGetSession(string sessionId, out IClientSession? session);

    /// <summary>
    /// Obtiene una lista de solo lectura con todas las sesiones actualmente conectadas.
    /// </summary>
    /// <returns>Colección de sesiones activas.</returns>
    IReadOnlyCollection<IClientSession> GetAllSessions();

    /// <summary>
    /// Obtiene el número total de conexiones activas en este instante.
    /// </summary>
    int ActiveConnectionsCount { get; }

    /// <summary>
    /// Difunde un mensaje textual a todas las sesiones activas conectadas.
    /// </summary>
    /// <param name="message">Cadena de mensaje a emitir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la operación de difusión.</returns>
    Task BroadcastAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Difunde un mensaje a todas las sesiones activas excepto a la sesión emisora especificada.
    /// </summary>
    /// <param name="message">Cadena de mensaje a emitir.</param>
    /// <param name="excludeSessionId">Identificador de la sesión a excluir de la transmisión.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la operación de difusión selectiva.</returns>
    Task BroadcastExceptAsync(string message, string excludeSessionId, CancellationToken cancellationToken = default);
}
