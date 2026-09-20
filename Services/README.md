# Capa de Servicios y Lógica de Negocio (`Services`)

La capa `Services` orquesta los casos de uso del servidor de juego, aplicando las reglas de validación, moderación, presencia de red y coordinación entre los repositorios de persistencia y la capa de transporte.

---

## Estructura del Módulo

```
Services/
├── UserService.cs              # Autenticación, presencia y actualización de posición
├── ChatService.cs             # Validación, moderación y persistencia de mensajes
├── Handlers/                  # Controladores por código de operación (SRP)
│   ├── AuthPacketHandler.cs
│   ├── PositionPacketHandler.cs
│   ├── ChatPacketHandler.cs
│   └── README.md
└── README.md
```

---

## Flujo de Datos

1. **Recepción**: La capa de red invoca a `PacketRouter`.
2. **Despacho**: `PacketRouter` entrega el payload al manejador correspondiente en `Services/Handlers/`.
3. **Ejecución**: El manejador delega en `UserService` o `ChatService`.
4. **Persistencia & Difusión**: Los servicios persisten en la base de datos y notifican a `IConnectionManager` para la difusión selectiva o global.
