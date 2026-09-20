namespace WebSocketServer.Protocol.Serialization;

/// <summary>
/// Define el contrato de serialización y deserialización para el protocolo de red.
/// </summary>
public interface INetworkSerializer
{
    /// <summary>
    /// Serializa un objeto a su representación en formato texto (JSON).
    /// </summary>
    /// <typeparam name="T">Tipo del objeto a serializar.</typeparam>
    /// <param name="value">Instancia a convertir.</param>
    /// <returns>Cadena formateada con los datos serializados.</returns>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializa una cadena textual a una instancia del tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de destino.</typeparam>
    /// <param name="serializedData">Cadena serializada.</param>
    /// <returns>Instancia reconstruida del tipo <typeparamref name="T"/> o <c>null</c>.</returns>
    T? Deserialize<T>(string serializedData);
}
