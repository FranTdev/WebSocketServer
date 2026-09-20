using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Core.Interfaces;

/// <summary>
/// Define el contrato de persistencia y consulta para el historial de mensajes de chat.
/// </summary>
public interface IChatRepository
{
    /// <summary>
    /// Persiste un mensaje de chat en el almacenamiento de datos.
    /// </summary>
    /// <param name="message">Entidad del mensaje a guardar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    Task SaveMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera los mensajes de chat más recientes para un canal específico, ordenados cronológicamente.
    /// </summary>
    /// <param name="channel">Canal a consultar (ej. "Global").</param>
    /// <param name="limit">Cantidad máxima de mensajes a devolver.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Lista de mensajes recuperados.</returns>
    Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(string channel = "Global", int limit = 50, CancellationToken cancellationToken = default);
}
