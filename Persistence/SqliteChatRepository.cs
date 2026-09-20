using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WebSocketServer.Core.Interfaces;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Persistence;

/// <summary>
/// Implementación de <see cref="IChatRepository"/> para persistencia de mensajes de chat en SQLite.
/// </summary>
public sealed class SqliteChatRepository : IChatRepository
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SqliteChatRepository"/>.
    /// </summary>
    /// <param name="connectionFactory">Fábrica de conexiones a la base de datos.</param>
    public SqliteChatRepository(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Persiste un mensaje de chat en la base de datos de forma asíncrona.
    /// </summary>
    /// <param name="message">Entidad del mensaje a guardar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    public async Task SaveMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO ChatMessages (Id, SenderId, SenderUsername, Content, Channel, TimestampUnixMilliseconds)
            VALUES ($id, $senderId, $senderUsername, $content, $channel, $timestamp);
        ";

        command.Parameters.AddWithValue("$id", message.Id);
        command.Parameters.AddWithValue("$senderId", message.SenderId);
        command.Parameters.AddWithValue("$senderUsername", message.SenderUsername);
        command.Parameters.AddWithValue("$content", message.Content);
        command.Parameters.AddWithValue("$channel", message.Channel);
        command.Parameters.AddWithValue("$timestamp", message.TimestampUnixMilliseconds);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene los mensajes más recientes de un canal ordenados cronológicamente de forma ascendente.
    /// </summary>
    /// <param name="channel">Canal a consultar.</param>
    /// <param name="limit">Cantidad máxima de mensajes.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de mensajes ordenados en orden cronológico ascendente.</returns>
    public async Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(string channel = "Global", int limit = 50, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>();
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        // Consulta los últimos 'limit' mensajes usando el índice descendente y los invierte para orden natural
        command.CommandText = @"
            SELECT Id, SenderId, SenderUsername, Content, Channel, TimestampUnixMilliseconds
            FROM ChatMessages
            WHERE Channel = $channel
            ORDER BY TimestampUnixMilliseconds DESC
            LIMIT $limit;
        ";

        command.Parameters.AddWithValue("$channel", channel);
        command.Parameters.AddWithValue("$limit", limit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            messages.Add(new ChatMessage
            {
                Id = reader.GetString(0),
                SenderId = reader.GetString(1),
                SenderUsername = reader.GetString(2),
                Content = reader.GetString(3),
                Channel = reader.GetString(4),
                TimestampUnixMilliseconds = reader.GetInt64(5)
            });
        }

        // Se invierte la lista para presentar los mensajes del más antiguo al más reciente
        messages.Reverse();
        return messages;
    }
}
