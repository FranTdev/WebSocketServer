using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Configuration;
using WebSocketServer.Network;
using WebSocketServer.Persistence;
using WebSocketServer.Protocol.Serialization;
using WebSocketServer.Services;
using WebSocketServer.Services.Handlers;

namespace WebSocketServer;

/// <summary>
/// Punto de entrada principal (Composition Root) para el servidor de videojuegos multijugador con WebSockets nativos.
/// </summary>
public static class Program
{
    /// <summary>
    /// Método de inicio asíncrono que orquesta la inicialización de la base de datos,
    /// inyección de dependencias y arranque del listener de red.
    /// </summary>
    /// <param name="args">Argumentos de la línea de comandos.</param>
    /// <returns>Tarea que representa el ciclo de vida completo de ejecución del servidor.</returns>
    public static async Task Main(string[] args)
    {
        try
        {
            Console.Title = "Game Server - Native .NET WebSocket & SQLite WAL";
        }
        catch
        {
            // Ignorar en entornos headless o contenedores donde Console.Title no está soportado
        }
        PrintBanner();

        // 1. Cargar Configuración Operativa
        var config = new ServerConfig();

        // 2. Inicialización de la Base de Datos Ultraligera SQLite en modo WAL
        Console.WriteLine("[Base de Datos] Inicializando motor SQLite...");
        var dbConnectionFactory = new DatabaseConnectionFactory(config);
        var dbInitializer = new DatabaseInitializer(dbConnectionFactory);
        await dbInitializer.InitializeAsync();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[Base de Datos] SQLite WAL inicializado y optimizado con éxito.");
        Console.ResetColor();

        // 3. Inicialización de Capa de Persistencia y Repositorios
        var userRepository = new SqliteUserRepository(dbConnectionFactory);
        var chatRepository = new SqliteChatRepository(dbConnectionFactory);

        // 4. Capa de Protocolo y Serialización
        var serializer = JsonNetworkSerializer.Instance;

        // 5. Capa de Red y Gestión de Sesiones
        var connectionManager = new ConnectionManager();
        var packetRouter = new PacketRouter(serializer);

        // 6. Capa de Lógica de Negocio y Servicios de Dominio
        var userService = new UserService(userRepository, connectionManager, serializer);
        var chatService = new ChatService(chatRepository, connectionManager, serializer);

        // 7. Registro Desacoplado de Manejadores de Paquetes (SRP)
        packetRouter.RegisterHandler(new AuthPacketHandler(userService, chatService, serializer));
        packetRouter.RegisterHandler(new PositionPacketHandler(userService, connectionManager, serializer));
        packetRouter.RegisterHandler(new ChatPacketHandler(chatService, serializer));

        // 8. Inicialización del Listener de Red WebSocket
        using var listener = new ServerWebSocketListener(config, connectionManager, packetRouter);

        listener.ClientConnected += (session) =>
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[Red] [+] Cliente conectado. Sesión: {session.SessionId} | Activas: {connectionManager.ActiveConnectionsCount}");
            Console.ResetColor();
        };

        listener.ClientDisconnected += (session) =>
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            string username = session.AuthenticatedUser?.Username ?? "Anónimo";
            Console.WriteLine($"[Red] [-] Cliente desconectado. Sesión: {session.SessionId} ({username}) | Activas: {connectionManager.ActiveConnectionsCount}");
            Console.ResetColor();

            _ = userService.NotifyUserLeftAsync(session);
        };

        listener.Start();

        // Manejo de apagado ordenado mediante Ctrl+C
        var exitEvent = new ManualResetEventSlim(false);
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            Console.WriteLine("\n[Servidor] Señal de terminación recibida. Cerrando ordenadamente...");
            exitEvent.Set();
        };

        Console.WriteLine("\n[Servidor] Listo y esperando conexiones de clientes Unity.");
        Console.WriteLine("[Servidor] Presiona Ctrl+C para detener el servidor.\n");

        exitEvent.Wait();

        Console.WriteLine("[Servidor] Deteniendo listener de red...");
        await listener.StopAsync();
        Console.WriteLine("[Servidor] Apagado completo.");
    }

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
===================================================================
       SERVIDOR DE VIDEOJUEGO MULTIJUGADOR (.NET WEBSOCKET)        
===================================================================
 Arquitectura: Clean Architecture & Modular Layering
 Protocolo   : JSON Envelopes sobre WebSockets nativos
 Base Datos  : SQLite nativa en modo WAL ultraligero
 Conectividad: Unity 3D Ready (Assets/Scripts/Networking/)
===================================================================
");
        Console.ResetColor();
    }
}