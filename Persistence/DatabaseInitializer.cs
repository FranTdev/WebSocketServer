using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Persistence;

/// <summary>
/// Inicializa el esquema de tablas, índices y pragmas de optimización WAL en SQLite.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DatabaseInitializer"/>.
    /// </summary>
    /// <param name="connectionFactory">Fábrica de conexiones a la base de datos.</param>
    public DatabaseInitializer(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Aplica las sentencias DDL para crear las tablas, índices y configurar el modo WAL.
    /// Si la base de datos está vacía, opcionalmente siembra un usuario administrador por defecto.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la inicialización asíncrona.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        // Optimización del motor SQLite para alta velocidad y concurrencia sin bloqueos
        var pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = @"
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
            PRAGMA temp_store = MEMORY;
            PRAGMA busy_timeout = 5000;
        ";
        await pragmaCommand.ExecuteNonQueryAsync(cancellationToken);

        // Creación de la tabla de usuarios
        var tableCommand = connection.CreateCommand();
        tableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY NOT NULL,
                Username TEXT UNIQUE NOT NULL COLLATE NOCASE,
                Role INTEGER NOT NULL,
                PosX REAL NOT NULL,
                PosY REAL NOT NULL,
                PosZ REAL NOT NULL,
                CreatedAt TEXT NOT NULL,
                LastActiveAt TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);

            CREATE TABLE IF NOT EXISTS ChatMessages (
                Id TEXT PRIMARY KEY NOT NULL,
                SenderId TEXT NOT NULL,
                SenderUsername TEXT NOT NULL,
                Content TEXT NOT NULL,
                Channel TEXT NOT NULL,
                TimestampUnixMilliseconds INTEGER NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_chat_channel_time ON ChatMessages(Channel, TimestampUnixMilliseconds DESC);
        ";
        await tableCommand.ExecuteNonQueryAsync(cancellationToken);

        // Sembrado inicial de un usuario administrador de ejemplo si la tabla está vacía
        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(1) FROM Users;";
        var countResult = await countCommand.ExecuteScalarAsync(cancellationToken);
        long count = countResult is long l ? l : Convert.ToInt64(countResult);

        if (count == 0)
        {
            var seedCommand = connection.CreateCommand();
            seedCommand.CommandText = @"
                INSERT INTO Users (Id, Username, Role, PosX, PosY, PosZ, CreatedAt, LastActiveAt)
                VALUES ($id, $username, $role, $posX, $posY, $posZ, $createdAt, $lastActiveAt);
            ";
            seedCommand.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
            seedCommand.Parameters.AddWithValue("$username", "ServerAdmin");
            seedCommand.Parameters.AddWithValue("$role", (int)UserRole.Admin);
            seedCommand.Parameters.AddWithValue("$posX", 0.0f);
            seedCommand.Parameters.AddWithValue("$posY", 1.0f);
            seedCommand.Parameters.AddWithValue("$posZ", 0.0f);
            seedCommand.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));
            seedCommand.Parameters.AddWithValue("$lastActiveAt", DateTime.UtcNow.ToString("o"));

            await seedCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
