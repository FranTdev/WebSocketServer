# Capa Núcleo / Dominio (`Core`)

La capa `Core` conforma el centro neurálgico de la solución siguiendo los principios de la Arquitectura Limpia (*Clean Architecture*). No depende de detalles de infraestructura, frameworks externos, WebSockets ni implementaciones concretas de bases de datos.

---

## Estructura de Directorios

```
Core/
├── Models/              # Entidades y objetos de valor del dominio (User, UserRole, Vector3Dto, ChatMessage)
│   └── README.md
├── Interfaces/          # Contratos e interfaces abstractas (IUserRepository, IChatRepository, etc.)
│   └── README.md
└── README.md
```

---

## Principios de Diseño Aplicados
1. **Regla de Dependencias**: El dominio no conoce la persistencia en SQLite ni el protocolo WebSocket. Todas las operaciones externas se delegan a través de contratos (`Interfaces`).
2. **Inmutabilidad y Eficiencia**: Los tipos espaciales como `Vector3Dto` son estructuras de solo lectura optimizadas para evitar reservas innecesarias en el recolector de basura (*GC*).
3. **Seguridad y Tipado**: Se descartan cadenas arbitrarias para roles o estados mediante enums (`UserRole`).
