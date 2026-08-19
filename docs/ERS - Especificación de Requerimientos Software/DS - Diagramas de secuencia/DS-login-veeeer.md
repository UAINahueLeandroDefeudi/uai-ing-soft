# DS - Login

Ingreso al sistema: validación de credenciales, política de bloqueo, carga del
árbol de permisos (Composite) y apertura de la sesión única (Singleton).

```mermaid
sequenceDiagram
    actor Usuario
    participant ML as Modulo Login
    participant SBLL as SesionBLL
    participant BLL as UsuarioBLL
    participant BE as UsuarioBE
    participant DAL as UsuarioDAL
    participant Enc as EncriptadorService
    participant PBLL as PermisosBLL
    participant Sess as SessionManagerService

    Usuario->>ML: IngresarAlSistema()
    ML-->>Usuario: MostrarInterfazLogin()
    Usuario->>+ML: Login(username, password)
    ML->>+SBLL: Login(string, string)
    SBLL->>+BLL: Buscar(string)
    BLL->>+DAL: BuscarPorUsername(string)
    DAL->>+BE: ArmarEntidad()
    BE-->>-DAL: Usuario
    DAL-->>-BLL: Usuario
    BLL-->>-SBLL: Usuario

    alt Usuario inexistente
        SBLL->>DAL: GuardarIntentoLogin(fallido)
        SBLL-->>ML: ResultadoLogin(CredencialInvalida)
        ML-->>Usuario: MostrarUsuarioOContraseñaIncorrecto()

    else Usuario bloqueado o inactivo
        SBLL->>SBLL: UsuarioBloqueado(): bool
        SBLL->>DAL: GuardarIntentoLogin(UsuarioBloqueado)
        SBLL-->>ML: ResultadoLogin(UsuarioBloqueado)
        ML-->>Usuario: MostrarMensajeBloqueo()

    else Usuario habilitado
        SBLL->>+BLL: ValidarCredencial(Usuario, password)
        BLL->>+Enc: Hash(password, salt)
        Enc-->>-BLL: hash: byte[]
        BLL-->>-SBLL: EsLoginValido(): bool

        alt Contraseña incorrecta
            SBLL->>+BLL: IncrementarIntentosFallidos(Usuario)
            BLL->>DAL: Actualizar(Usuario)
            BLL-->>-SBLL: intentos: int
            opt intentos >= 3
                SBLL->>BLL: Bloquear(Usuario)
            end
            SBLL-->>ML: ResultadoLogin(CredencialInvalida)
            ML-->>Usuario: MostrarUsuarioOContraseñaIncorrecto()

        else Credenciales correctas
            SBLL->>BLL: ResetearIntentos(Usuario)
            SBLL->>+PBLL: FillUserComponents(Usuario)
            Note over PBLL: Arma el árbol Composite<br/>Familias y Patentes del usuario
            PBLL-->>-SBLL: Usuario.Permisos cargados
            SBLL->>+Sess: Login(Usuario)
            Note over Sess: crea la instancia única<br/>de la sesión (Singleton)
            Sess-->>-SBLL: SesionIniciada()
            SBLL->>DAL: GuardarIntentoLogin(exitoso)
            SBLL-->>-ML: ResultadoLogin(EsValido = true)
            ML-->>-Usuario: MostrarMenuPrincipal()
        end
    end
```

## Detalle

| Paso | Regla |
|---|---|
| Búsqueda del usuario | Se busca por `username`; el `password` nunca viaja a la DAL en claro |
| Validación | `EncriptadorService` calcula el hash con el `salt` del usuario y lo compara |
| Intentos fallidos | Se incrementan y persisten; al llegar a 3 el usuario queda `bloqueado` |
| Mensaje al usuario | Es el mismo para "usuario inexistente" y "contraseña incorrecta" (no se le informa al atacante cuál de los dos falló) |
| Permisos | Se cargan **una sola vez** al iniciar sesión — ver [DS-permisos-composite.md](DS-permisos-composite.md) |
| Sesión | La abre `SessionManager`, instancia única — ver [DS-sesion-singleton.md](DS-sesion-singleton.md) |

## Diagramas relacionados

- Clases: [DC-login.md](../DC%20-%20Diagrama%20de%20clases/DC-login.md)
- Datos: [DER-login.md](../DER%20-%20Diagrama%20entidad%20relación/DER-login.md)
- Caso de uso: [CU-login.md](../CU%20-%20Casos%20de%20uso/CU-login.md)
