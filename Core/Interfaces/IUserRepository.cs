using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Core.Interfaces;

/// <summary>
/// Define el contrato de persistencia y consulta para entidades de usuario.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtiene un usuario a partir de su identificador único.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>La entidad <see cref="User"/> si existe; de lo contrario, <c>null</c>.</returns>
    Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario a partir de su nombre de usuario.
    /// </summary>
    /// <param name="username">Nombre de usuario a consultar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>La entidad <see cref="User"/> si existe; de lo contrario, <c>null</c>.</returns>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda un nuevo usuario o actualiza sus propiedades si ya se encuentra registrado.
    /// </summary>
    /// <param name="user">Entidad de usuario a persistir.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    Task UpsertAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza exclusivamente la posición espacial y la marca de última actividad de un usuario.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="position">Nueva posición tridimensional.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    Task UpdatePositionAsync(string userId, Vector3Dto position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los usuarios registrados en el sistema.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Colección enumerable de usuarios.</returns>
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
}
