using System;
using System.Text.Json.Serialization;

namespace WebSocketServer.Core.Models;

/// <summary>
/// Representa una posición o vector tridimensional en el espacio cartesiano para videojuegos en red.
/// </summary>
public readonly struct Vector3Dto : IEquatable<Vector3Dto>
{
    /// <summary>
    /// Obtiene el componente en el eje horizontal X.
    /// </summary>
    [JsonPropertyName("x")]
    public float X { get; }

    /// <summary>
    /// Obtiene el componente en el eje vertical Y (altura).
    /// </summary>
    [JsonPropertyName("y")]
    public float Y { get; }

    /// <summary>
    /// Obtiene el componente en el eje de profundidad Z.
    /// </summary>
    [JsonPropertyName("z")]
    public float Z { get; }

    /// <summary>
    /// Inicializa una nueva instancia de la estructura <see cref="Vector3Dto"/> con las coordenadas especificadas.
    /// </summary>
    /// <param name="x">Componente en el eje X.</param>
    /// <param name="y">Componente en el eje Y.</param>
    /// <param name="z">Componente en el eje Z.</param>
    [JsonConstructor]
    public Vector3Dto(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Vector nulo que representa el origen de coordenadas (0, 0, 0).
    /// </summary>
    public static Vector3Dto Zero => new(0f, 0f, 0f);

    /// <summary>
    /// Calcula la distancia euclidiana al cuadrado entre dos posiciones.
    /// Optimizado para evitar raíces cuadradas costosas en validaciones de red.
    /// </summary>
    /// <param name="a">Primera posición.</param>
    /// <param name="b">Segunda posición.</param>
    /// <returns>Distancia euclidiana elevada al cuadrado.</returns>
    public static float SqrDistance(Vector3Dto a, Vector3Dto b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        float dz = a.Z - b.Z;
        return (dx * dx) + (dy * dy) + (dz * dz);
    }

    /// <summary>
    /// Determina si la instancia actual es igual a otro objeto <see cref="Vector3Dto"/>.
    /// </summary>
    /// <param name="other">Objeto con el cual comparar.</param>
    /// <returns><c>true</c> si son idénticos; en caso contrario, <c>false</c>.</returns>
    public bool Equals(Vector3Dto other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    /// <summary>
    /// Determina si un objeto genérico es equivalente a la instancia actual.
    /// </summary>
    /// <param name="obj">Objeto con el cual comparar.</param>
    /// <returns><c>true</c> si el objeto es un <see cref="Vector3Dto"/> equivalente; de lo contrario <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Vector3Dto other && Equals(other);
    }

    /// <summary>
    /// Retorna el código hash de la instancia calculando los componentes espaciales.
    /// </summary>
    /// <returns>Código hash entero.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    /// <summary>
    /// Compara la igualdad de dos vectores tridimensionales.
    /// </summary>
    public static bool operator ==(Vector3Dto left, Vector3Dto right) => left.Equals(right);

    /// <summary>
    /// Compara la desigualdad de dos vectores tridimensionales.
    /// </summary>
    public static bool operator !=(Vector3Dto left, Vector3Dto right) => !left.Equals(right);

    /// <summary>
    /// Devuelve una representación textual de las coordenadas.
    /// </summary>
    /// <returns>Cadena en formato (X, Y, Z).</returns>
    public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
}
