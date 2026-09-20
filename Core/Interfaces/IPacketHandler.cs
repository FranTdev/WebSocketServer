using System.Threading;
using System.Threading.Tasks;

namespace WebSocketServer.Core.Interfaces;

/// <summary>
/// Define el contrato para procesadores desacoplados de paquetes de red específicos.
/// </summary>
public interface IPacketHandler
{
    /// <summary>
    /// Obtiene el código de operación numérico (OpCode) que este manejador es capaz de procesar.
    /// </summary>
    short OpCode { get; }

    /// <summary>
    /// Procesa el paquete recibido desde la sesión de cliente especificada.
    /// </summary>
    /// <param name="session">Sesión del cliente que originó la solicitud.</param>
    /// <param name="payloadJson">Cuerpo serializado en JSON del contenido del paquete.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa el procesamiento asíncrono del mensaje.</returns>
    Task HandleAsync(IClientSession session, string payloadJson, CancellationToken cancellationToken = default);
}
