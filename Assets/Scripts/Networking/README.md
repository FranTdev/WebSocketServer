# Paquete de Red para Unity (`Assets/Scripts/Networking/`)

Módulo completo, modular y de producción para la integración de red cliente-servidor multijugador en Unity utilizando WebSockets nativos de .NET (`System.Net.WebSockets.ClientWebSocket`) sin librerías externas.

---

## Cómo Integrar en tu Proyecto de Unity

1. **Copiar Carpeta**:
   - Copia la carpeta `Networking` completa a la ruta `Assets/Scripts/Networking/` dentro de tu proyecto de Unity.

2. **Configurar el `NetworkManager`**:
   - En una escena inicial o de arranque (*Bootstrapping*), crea un nuevo GameObject vacío y nómbralo `[NetworkManager]`.
   - Añade el componente `NetworkManager.cs`.
   - En el inspector, configura la propiedad **Server Url**:
     ```
     ws://localhost:8080/
     ```
     *(o la IP local/pública de tu servidor .NET)*.
   - Activa opcionalmente la casilla **Auto Connect On Start**.

3. **Sincronización de Jugador (`PlayerNetworkSync`)**:
   - En el prefab de tu jugador local:
     - Añade el componente `PlayerNetworkSync.cs`.
     - Deja la casilla **Is Local Player** marcada en `true`.
     - Ajusta **Sync Rate Hz** (recomendado: 20 Hz para juegos de ritmo medio o 30 Hz para acción rápida).
   - En el prefab de los jugadores remotos:
     - Añade el componente `PlayerNetworkSync.cs`.
     - Desmarca la casilla **Is Local Player** (`false`).
     - Al instanciarlo mediante el evento `OnUserJoined` o `OnUserListSynchronized`, asigna la propiedad `RemoteUserId`.

4. **Interfaz de Chat (`ChatUIController`)**:
   - En tu Canvas de UI, añade un GameObject con el componente `ChatUIController.cs`.
   - Asigna las referencias en el inspector:
     - `Chat Input Field`: Campo `InputField` para redactar.
     - `Send Button`: Botón `Button` para enviar.
     - `Chat Display Area`: Componente `Text` dentro del ScrollView.
     - `Chat Scroll Rect`: Componente `ScrollRect` para el autodesplazamiento.

---

## Ejemplo Rápido de Uso desde Código

```csharp
using UnityEngine;
using Networking.Runtime.Controllers;

public class GameLauncher : MonoBehaviour
{
    private async void Start()
    {
        // 1. Conectar al servidor
        await NetworkManager.Instance.ConnectAsync();

        // 2. Iniciar sesión como jugador
        await NetworkManager.Instance.AuthenticateAsync("FranGamer", 1);

        // 3. Enviar un saludo en el chat
        await NetworkManager.Instance.SendChatMessageAsync("¡Hola a todos desde Unity!");
    }
}
```

---

## Arquitectura y Desacoplamiento

- **Seguridad en Hilos**: Toda la recepción asíncrona pasa por `UnityMainThreadDispatcher` para garantizar que nunca se invoquen métodos de Unity fuera del hilo principal.
- **Tolerancia a Concurrencia**: Los envíos utilizan `SemaphoreSlim(1,1)` interno para evitar colisiones en el socket.
- **Serialización Nativa**: Los DTOs son serializables y compatibles con `JsonUtility`, minimizando el consumo de CPU y memoria en dispositivos móviles y PC.
