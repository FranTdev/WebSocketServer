using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Configuration;
using WebSocketServer.Core.Interfaces;

namespace WebSocketServer.Network;

/// <summary>
/// Listener de red principal que gestiona el socket HTTP de entrada, acepta solicitudes de handshake WebSocket
/// y delega las conexiones aceptadas a sesiones de cliente y al enrutador de paquetes.
/// </summary>
public sealed class ServerWebSocketListener : IDisposable
{
    private readonly ServerConfig _config;
    private readonly IConnectionManager _connectionManager;
    private readonly PacketRouter _packetRouter;
    private readonly HttpListener _httpListener;
    private CancellationTokenSource? _cts;
    private Task? _listenerLoopTask;

    /// <summary>
    /// Evento emitido cuando un nuevo cliente completa el handshake WebSocket y se conecta al servidor.
    /// </summary>
    public event Action<IClientSession>? ClientConnected;

    /// <summary>
    /// Evento emitido cuando una sesión activa de cliente se desconecta.
    /// </summary>
    public event Action<IClientSession>? ClientDisconnected;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ServerWebSocketListener"/>.
    /// </summary>
    /// <param name="config">Configuración operativa del servidor.</param>
    /// <param name="connectionManager">Administrador de sesiones activas.</param>
    /// <param name="packetRouter">Enrutador de paquetes.</param>
    public ServerWebSocketListener(ServerConfig config, IConnectionManager connectionManager, PacketRouter packetRouter)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _packetRouter = packetRouter ?? throw new ArgumentNullException(nameof(packetRouter));

        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add(_config.ListenerPrefix);
    }

    /// <summary>
    /// Inicia la escucha de conexiones entrantes de forma no bloqueante.
    /// </summary>
    public void Start()
    {
        if (_httpListener.IsListening)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        _httpListener.Start();
        _listenerLoopTask = Task.Run(() => ListenLoopAsync(_cts.Token));

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Listener] Servidor WebSocket escuchando en {_config.ListenerPrefix}");
        Console.ResetColor();
    }

    /// <summary>
    /// Detiene la escucha y desconecta de forma ordenada todas las sesiones activas.
    /// </summary>
    /// <returns>Tarea que representa el apagado ordenado del listener.</returns>
    public async Task StopAsync()
    {
        if (!_httpListener.IsListening)
        {
            return;
        }

        _cts?.Cancel();

        try
        {
            _httpListener.Stop();
        }
        catch
        {
            // Ignorar errores al detener el listener de bajo nivel
        }

        if (_listenerLoopTask != null)
        {
            try
            {
                await _listenerLoopTask;
            }
            catch (OperationCanceledException)
            {
                // Cancelación esperada
            }
        }

        // Desconectar sesiones activas
        foreach (var session in _connectionManager.GetAllSessions())
        {
            await session.DisconnectAsync("El servidor se está apagando.", CancellationToken.None);
        }
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _httpListener.IsListening)
        {
            try
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    if (_connectionManager.ActiveConnectionsCount >= _config.MaxConcurrentConnections)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[Listener] Conexión rechazada: límite máximo de conexiones alcanzado.");
                        Console.ResetColor();
                        context.Response.StatusCode = 503; // Service Unavailable
                        context.Response.Close();
                        continue;
                    }

                    _ = ProcessWebSocketHandshakeAsync(context, cancellationToken);
                }
                else
                {
                    // Si no es un handshake WebSocket, se retorna 400 Bad Request
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Listener] Error aceptando petición HTTP/WS: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
    }

    private async Task ProcessWebSocketHandshakeAsync(HttpListenerContext context, CancellationToken cancellationToken)
    {
        HttpListenerWebSocketContext wsContext;
        try
        {
            wsContext = await context.AcceptWebSocketAsync(
                subProtocol: null, 
                receiveBufferSize: _config.ReceiveBufferSize, 
                keepAliveInterval: TimeSpan.FromSeconds(_config.KeepAliveIntervalSeconds)
            );
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Listener] Error durante el handshake de WebSocket: {ex.Message}");
            Console.ResetColor();
            return;
        }

        var session = new ClientSession(wsContext.WebSocket, _config.ReceiveBufferSize);

        // Suscripción al bucle de mensajes
        session.MessageReceived += async (senderSession, rawMessage) =>
        {
            await _packetRouter.RoutePacketAsync(senderSession, rawMessage, cancellationToken);
        };

        // Suscripción al evento de desconexión
        session.Disconnected += (disconnectedSession) =>
        {
            _connectionManager.RemoveSession(disconnectedSession.SessionId);
            ClientDisconnected?.Invoke(disconnectedSession);
        };

        _connectionManager.AddSession(session);
        ClientConnected?.Invoke(session);

        // Iniciar la tarea de lectura continua de la sesión
        _ = Task.Run(() => session.StartReceiveLoopAsync(cancellationToken), cancellationToken);
    }

    /// <summary>
    /// Libera los recursos del socket listener HTTP.
    /// </summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _httpListener.Close();
    }
}
