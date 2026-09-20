using System.Text.Json;

namespace WebSocketServer.Protocol.Serialization;

/// <summary>
/// Implementación de alto rendimiento basada en <see cref="System.Text.Json"/> para el protocolo de red.
/// </summary>
public sealed class JsonNetworkSerializer : INetworkSerializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Instancia estática reutilizable y thread-safe para operaciones globales del protocolo.
    /// </summary>
    public static JsonNetworkSerializer Instance { get; } = new();

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="JsonNetworkSerializer"/> configurando opciones de serialización óptimas.
    /// </summary>
    public JsonNetworkSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Serializa cualquier entidad o paquete a formato JSON compacto.
    /// </summary>
    /// <typeparam name="T">Tipo del objeto.</typeparam>
    /// <param name="value">Instancia a serializar.</param>
    /// <returns>Cadena JSON compacta.</returns>
    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }

    /// <summary>
    /// Deserializa una cadena JSON al tipo fuertemente tipado especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de destino.</typeparam>
    /// <param name="serializedData">Texto en formato JSON.</param>
    /// <returns>Instancia deserializada o <c>null</c> si el texto no es válido.</returns>
    public T? Deserialize<T>(string serializedData)
    {
        if (string.IsNullOrWhiteSpace(serializedData))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(serializedData, _options);
    }
}
