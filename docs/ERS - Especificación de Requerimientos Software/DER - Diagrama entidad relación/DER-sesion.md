# DER - Sesión (SessionManager)

`SessionManager` es un **Singleton en memoria**: dentro del proceso existe una y
sólo una sesión activa. La base de datos **no** almacena el objeto sesión, sino
su *traza*: cuándo se abrió, con qué usuario, desde qué host/proceso y cuándo se
cerró. Eso permite auditar, detectar sesiones abiertas sin cierre e impedir el
login simultáneo del mismo usuario desde dos puestos.

```mermaid
erDiagram
    USUARIO ||--o{ SESION         : "abre"
    SESION  ||--o{ SESION_EVENTO  : "produce"
    SESION  ||--o{ BITACORA       : "contextualiza"
    PERMISO ||--o{ SESION_PERMISO : "se congela en"
    SESION  ||--o{ SESION_PERMISO : "cachea"

    SESION {
        uniqueidentifier id_sesion PK "GUID generado en el Login"
        int      id_usuario        FK
        datetime fecha_inicio         "SessionManager.FechaInicio"
        datetime fecha_ultimo_acceso  "Renovada en cada operación"
        datetime fecha_fin            "NULL mientras está activa"
        varchar  estado               "Activa / Cerrada / Expirada / Forzada"
        varchar  host                 "Environment.MachineName"
        int      id_proceso           "PID que aloja el Singleton"
        varchar  motivo_cierre        "Logout / Timeout / Cierre de app"
    }

    SESION_EVENTO {
        bigint   id_evento PK
        uniqueidentifier id_sesion FK
        datetime fecha_hora
        varchar  tipo      "Login / Logout / AccesoDenegado / Renovacion"
        varchar  detalle
    }

    SESION_PERMISO {
        uniqueidentifier id_sesion PK
        int id_permiso            PK "Permiso efectivo resuelto al iniciar sesión"
    }

    USUARIO {
        int     id_usuario PK
        varchar username   UK
        bit     activo
    }

    PERMISO {
        int     id     PK
        varchar nombre
        varchar permiso "NULL = Familia"
    }

    BITACORA {
        bigint   id_bitacora PK
        uniqueidentifier id_sesion FK
        datetime fecha_hora
        varchar  modulo
        varchar  criticidad
        varchar  descripcion
    }
```

## Reglas

| Regla | Motivo |
|---|---|
| A lo sumo una `SESION` con `estado = 'Activa'` por proceso | Es el reflejo en BD de la instancia única del Singleton |
| `UNIQUE (id_usuario) WHERE estado = 'Activa'` (índice filtrado) | Evita login simultáneo del mismo usuario en dos máquinas |
| `fecha_fin IS NULL` ⟺ `estado = 'Activa'` | Consistencia del ciclo de vida |
| Al arrancar la app se cierran las sesiones huérfanas del mismo `host`+`id_proceso` | Recuperación ante cierre abrupto |
| `SESION_PERMISO` es opcional (caché) | Si se prefiere permiso "vivo", se recalcula del árbol Composite en cada chequeo |
