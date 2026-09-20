using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Core.Interfaces;

/// <summary>
/// Representa el contrato de una sesión de cliente conectada al servidor a través de un canal de red dúplex.
/// </summary>
public interface IClientSession : IDisposable
{
    /// <summary>
    /// Identificador único de la sesión de red.
    /// </summary>
    string SessionId { get; }

    /// <summary>
    /// Usuario autenticado asociado a esta sesión; es <c>null</c> si la sesión aún no ha sido autenticada.
    /// </summary>
    User? AuthenticatedUser { get; }

    /// <summary>
    /// Indica si la conexión subyacente se encuentra abierta y lista para transmitir.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Asocia un usuario verificado a la sesión actual.
    /// </summary>
    /// <param name="user">Entidad de usuario autenticado.</param>
    void AssociateUser(User user);

    /// <summary>
    /// Envía un mensaje textual (normalmente un paquete JSON serializado) de forma segura y no bloqueante.
    /// </summary>
    /// <param name="message">Cadena de texto a transmitir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la operación de envío.</returns>
    Task SendTextAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cierra la conexión de la sesión con el estado y motivo especificados.
    /// </summary>
    /// <param name="reason">Motivo del cierre.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la operación de cierre.</returns>
    Task DisconnectAsync(string reason, CancellationToken cancellationToken = default);
}
