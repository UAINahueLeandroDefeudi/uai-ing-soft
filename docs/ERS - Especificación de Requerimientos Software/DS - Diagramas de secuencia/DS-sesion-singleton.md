# DS - Sesión (Singleton)

Ciclo de vida de la instancia única de `SessionManager`: creación, uso durante la
aplicación y destrucción.

## 1. Instanciación de la sesión

```mermaid
sequenceDiagram
    participant SBLL as SesionBLL
    participant SM as SessionManager
    participant U as UsuarioBE
    participant DAL as SesionDAL

    Note over SM: _session = null

    SBLL->>+SM: Login(Usuario)
    alt No hay sesión iniciada
        SM->>SM: new SessionManager()
        SM->>+U: Usuario
        U-->>-SM: datos y permisos
        SM->>SM: _session.Usuario = Usuario
        SM->>SM: _session.FechaInicio = DateTime.Now
        SM->>+DAL: AbrirSesion(Sesion)
        DAL-->>-SM: OK
        SM-->>SBLL: sesión iniciada
    else Ya existe una sesión
        SM--x SBLL: throw Sesión ya iniciada
    end
    deactivate SM

    Note over SM: Existe una y sólo una instancia
```

## 2. Uso de la sesión durante la aplicación

```mermaid
sequenceDiagram
    actor Usuario
    participant UI as FrmMain
    participant SM as SessionManager
    participant U as UsuarioBE

    Usuario->>+UI: AbrirModuloSeguridad()
    UI->>+SM: GetInstance
    alt Hay sesión iniciada
        SM-->>UI: SessionManager
        UI->>SM: IsInRole(TipoPermiso.PermisoAsignar)
        SM->>+U: Permisos
        U-->>-SM: árbol Composite
        SM-->>-UI: true / false
        UI-->>Usuario: Habilita o deshabilita el menú
    else No hay sesión iniciada
        SM--x UI: throw Sesión no iniciada
        UI-->>-Usuario: RedirigirALogin()
    end
```

## 3. Cierre de sesión

```mermaid
sequenceDiagram
    actor Usuario
    participant UI as FrmMain
    participant SBLL as SesionBLL
    participant SM as SessionManager
    participant DAL as SesionDAL

    Usuario->>+UI: CerrarSesion()
    UI->>+SBLL: Logout()
    SBLL->>+SM: GetInstance.IdSesion
    SM-->>-SBLL: Guid
    SBLL->>+SM: Logout()
    alt Hay sesión iniciada
        SM->>SM: _session = null
        SM-->>SBLL: OK
        SBLL->>+DAL: CerrarSesion(idSesion, Logout)
        DAL-->>-SBLL: OK
    else No hay sesión iniciada
        SM--x SBLL: throw Sesión no iniciada
    end
    deactivate SM
    SBLL-->>-UI: SesionCerrada
    UI-->>-Usuario: MostrarInterfazLogin()
```

## Puntos clave

| Situación | Comportamiento |
|---|---|
| `Login` sin sesión previa | Crea la instancia con el usuario y la fecha de inicio |
| `Login` con sesión ya iniciada | Lanza excepción: no se permite una segunda sesión |
| `GetInstance` sin sesión | Lanza excepción: el sistema redirige al log-in |
| `Logout` con sesión activa | Destruye la instancia y cierra la sesión en la base |
| `Logout` sin sesión | Lanza excepción |
| Cierre abrupto de la aplicación | La sesión queda abierta en la base y se depura en el próximo arranque |

## Diagramas relacionados

- Clases: [DC-sesion-singleton.md](../DC%20-%20Diagrama%20de%20clases/DC-sesion-singleton.md)
- Datos: [DER-sesion.md](../DER%20-%20Diagrama%20entidad%20relación/DER-sesion.md)
- Login: [DS-login.md](DS-login.md)
