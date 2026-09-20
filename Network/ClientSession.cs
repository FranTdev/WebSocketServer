using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.Interfaces;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Network;

/// <summary>
/// Encapsula una conexión de WebSocket individual para un cliente conectado, gestionando su ciclo de vida,
/// recepción continua y envío thread-safe no bloqueante mediante un semáforo de exclusión mutua.
/// </summary>
public sealed class ClientSession : IClientSession
{
    private readonly WebSocket _webSocket;
    private readonly int _bufferSize;
    private readonly SemaphoreSlim _sendSemaphore = new(1, 1);
    private int _isDisposed;

    /// <summary>
    /// Evento disparado de forma asíncrona cuando se recibe un mensaje de texto completo desde el socket del cliente.
    /// </summary>
    public event Func<IClientSession, string, Task>? MessageReceived;

    /// <summary>
    /// Evento disparado cuando la sesión de cliente se desconecta o la conexión se interrumpe.
    /// </summary>
    public event Action<IClientSession>? Disconnected;

    /// <summary>
    /// Identificador único de la sesión generado al momento de la conexión.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Usuario autenticado asociado a la sesión. Es <c>null</c> hasta que se complete el handshake de autenticación.
    /// </summary>
    public User? AuthenticatedUser { get; private set; }

    /// <summary>
    /// Determina si la conexión WebSocket subyacente se encuentra en estado abierto.
    /// </summary>
    public bool IsConnected => _webSocket.State == WebSocketState.Open;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ClientSession"/>.
    /// </summary>
    /// <param name="webSocket">Instancia activa de <see cref="WebSocket"/>.</param>
    /// <param name="bufferSize">Tamaño del búfer de lectura en bytes.</param>
    public ClientSession(WebSocket webSocket, int bufferSize = 4096)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _bufferSize = bufferSize > 0 ? bufferSize : 4096;
        SessionId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Asocia un usuario verificado por la capa de servicios a esta sesión de red.
    /// </summary>
    /// <param name="user">Entidad de usuario autenticado.</param>
    public void AssociateUser(User user)
    {
        AuthenticatedUser = user ?? throw new ArgumentNullException(nameof(user));
    }

    /// <summary>
    /// Inicia el bucle asíncrono de recepción de paquetes. Debe ejecutarse en una tarea desacoplada.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación global.</param>
    /// <returns>Tarea que representa el ciclo de escucha continuo de la sesión.</returns>
    public async Task StartReceiveLoopAsync(CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[_bufferSize];

        try
        {
            while (IsConnected && !cancellationToken.IsCancellationRequested)
            {
                using var memoryStream = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync("Cliente solicitó desconexión normal.", cancellationToken);
                        return;
                    }

                    memoryStream.Write(buffer, 0, result.Count);

                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string textMessage = Encoding.UTF8.GetString(memoryStream.ToArray());

                    if (MessageReceived != null)
                    {
                        await MessageReceived.Invoke(this, textMessage);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelación intencionada por cierre de servidor
        }
        catch (WebSocketException)
        {
            // Desconexión abrupta por pérdida de red o reinicio del cliente
        }
        catch (Exception)
        {
            // Errores de transporte inesperados
        }
        finally
        {
            OnDisconnected();
        }
    }

    /// <summary>
    /// Envía una trama de texto al cliente garantizando exclusión mutua para evitar violaciones de concurrencia en el WebSocket.
    /// </summary>
    /// <param name="message">Texto en formato JSON o protocolo a enviar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la transmisión segura del mensaje.</returns>
    public async Task SendTextAsync(string message, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            return;
        }

        byte[] bytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(bytes);

        await _sendSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
            {
                await _webSocket.SendAsync(segment, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
            }
        }
        catch (WebSocketException)
        {
            // El cliente cerró o interrumpió la conexión durante la transmisión
        }
        catch (ObjectDisposedException)
        {
            // Socket ya liberado
        }
        finally
        {
            _sendSemaphore.Release();
        }
    }

    /// <summary>
    /// Cierra ordenadamente la conexión WebSocket si aún se encuentra en un estado activo.
    /// </summary>
    /// <param name="reason">Motivo o causa de la desconexión.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa el cierre asíncrono.</returns>
    public async Task DisconnectAsync(string reason, CancellationToken cancellationToken = default)
    {
        if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, cancellationToken);
            }
            catch
            {
                // Conexión posiblemente ya cerrada por el extremo remoto
            }
        }

        OnDisconnected();
    }

    private void OnDisconnected()
    {
        Disconnected?.Invoke(this);
    }

    /// <summary>
    /// Libera los recursos no administrados del WebSocket y los mecanismos de sincronización.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
        {
            return;
        }

        try
        {
            _sendSemaphore.Dispose();
            _webSocket.Dispose();
        }
        catch
        {
            // Evitar excepciones al liberar sockets interrumpidos
        }
    }
}
