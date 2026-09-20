using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WebSocketServer.Configuration;

namespace WebSocketServer.Persistence;

/// <summary>
/// Fábrica de conexiones para la base de datos SQLite embebida de alto rendimiento.
/// </summary>
public sealed class DatabaseConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DatabaseConnectionFactory"/> a partir de la configuración.
    /// </summary>
    /// <param name="config">Configuración del servidor con la cadena de conexión.</param>
    public DatabaseConnectionFactory(ServerConfig config)
    {
        _connectionString = config.DatabaseConnectionString;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DatabaseConnectionFactory"/> con una cadena de conexión explícita.
    /// </summary>
    /// <param name="connectionString">Cadena de conexión SQLite.</param>
    public DatabaseConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Crea y abre de forma asíncrona una nueva conexión SQLite configurada para alta concurrencia.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Instancia de <see cref="SqliteConnection"/> en estado abierto.</returns>
    public async Task<SqliteConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        EnsureDatabaseDirectoryExists(_connectionString);
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static void EnsureDatabaseDirectoryExists(string connectionString)
    {
        try
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            if (!string.IsNullOrWhiteSpace(builder.DataSource) && builder.DataSource != ":memory:")
            {
                string? dir = System.IO.Path.GetDirectoryName(builder.DataSource);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
            }
        }
        catch
        {
            // Ignorar excepciones de formato o permisos que serán gestionadas por SqliteConnection
        }
    }
}
