using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WebSocketServer.Core.Interfaces;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Persistence;

/// <summary>
/// Implementación de <see cref="IUserRepository"/> sobre SQLite nativo de alto rendimiento.
/// </summary>
public sealed class SqliteUserRepository : IUserRepository
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="SqliteUserRepository"/>.
    /// </summary>
    /// <param name="connectionFactory">Fábrica de conexiones SQLite.</param>
    public SqliteUserRepository(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Obtiene un usuario a partir de su identificador único.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Instancia de <see cref="User"/> si existe; de lo contrario, <c>null</c>.</returns>
    public async Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT Id, Username, Role, PosX, PosY, PosZ, CreatedAt, LastActiveAt
            FROM Users
            WHERE Id = $id
            LIMIT 1;
        ";
        command.Parameters.AddWithValue("$id", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapUser(reader);
        }

        return null;
    }

    /// <summary>
    /// Obtiene un usuario a partir de su nombre de usuario (sin distinción de mayúsculas/minúsculas).
    /// </summary>
    /// <param name="username">Nombre de usuario a consultar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Instancia de <see cref="User"/> si existe; de lo contrario, <c>null</c>.</returns>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT Id, Username, Role, PosX, PosY, PosZ, CreatedAt, LastActiveAt
            FROM Users
            WHERE Username = $username
            LIMIT 1;
        ";
        command.Parameters.AddWithValue("$username", username);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapUser(reader);
        }

        return null;
    }

    /// <summary>
    /// Inserta o actualiza un usuario utilizando la instrucción de conflicto nativa de SQLite (UPSERT).
    /// </summary>
    /// <param name="user">Entidad a guardar o actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    public async Task UpsertAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Users (Id, Username, Role, PosX, PosY, PosZ, CreatedAt, LastActiveAt)
            VALUES ($id, $username, $role, $posX, $posY, $posZ, $createdAt, $lastActiveAt)
            ON CONFLICT(Id) DO UPDATE SET
                Username = excluded.Username,
                Role = excluded.Role,
                PosX = excluded.PosX,
                PosY = excluded.PosY,
                PosZ = excluded.PosZ,
                LastActiveAt = excluded.LastActiveAt;
        ";

        command.Parameters.AddWithValue("$id", user.Id);
        command.Parameters.AddWithValue("$username", user.Username);
        command.Parameters.AddWithValue("$role", (int)user.Role);
        command.Parameters.AddWithValue("$posX", (double)user.Position.X);
        command.Parameters.AddWithValue("$posY", (double)user.Position.Y);
        command.Parameters.AddWithValue("$posZ", (double)user.Position.Z);
        command.Parameters.AddWithValue("$createdAt", user.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("$lastActiveAt", user.LastActiveAt.ToString("o"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Actualiza únicamente las coordenadas espaciales y la marca de actividad de un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="position">Nuevas coordenadas 3D.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea asíncrona.</returns>
    public async Task UpdatePositionAsync(string userId, Vector3Dto position, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Users
            SET PosX = $posX, PosY = $posY, PosZ = $posZ, LastActiveAt = $lastActiveAt
            WHERE Id = $id;
        ";

        command.Parameters.AddWithValue("$id", userId);
        command.Parameters.AddWithValue("$posX", (double)position.X);
        command.Parameters.AddWithValue("$posY", (double)position.Y);
        command.Parameters.AddWithValue("$posZ", (double)position.Z);
        command.Parameters.AddWithValue("$lastActiveAt", DateTime.UtcNow.ToString("o"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Recupera la lista completa de usuarios persistidos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Colección de usuarios.</returns>
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<User>();
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT Id, Username, Role, PosX, PosY, PosZ, CreatedAt, LastActiveAt
            FROM Users;
        ";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(MapUser(reader));
        }

        return list;
    }

    private static User MapUser(SqliteDataReader reader)
    {
        string id = reader.GetString(0);
        string username = reader.GetString(1);
        UserRole role = (UserRole)reader.GetInt32(2);
        float posX = (float)reader.GetDouble(3);
        float posY = (float)reader.GetDouble(4);
        float posZ = (float)reader.GetDouble(5);
        DateTime createdAt = DateTime.Parse(reader.GetString(6), null, DateTimeStyles.RoundtripKind);
        DateTime lastActiveAt = DateTime.Parse(reader.GetString(7), null, DateTimeStyles.RoundtripKind);

        return new User
        {
            Id = id,
            Username = username,
            Role = role,
            Position = new Vector3Dto(posX, posY, posZ),
            CreatedAt = createdAt,
            LastActiveAt = lastActiveAt
        };
    }
}
