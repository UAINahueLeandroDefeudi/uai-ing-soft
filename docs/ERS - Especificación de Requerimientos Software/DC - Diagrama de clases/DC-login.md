# DC - Diagrama de clases: Login

Arquitectura en capas (UI → BLL → DAL → BE) del Caso de Uso Login.
La UI **no** conoce la DAL y la BLL **no** conoce controles visuales.

```mermaid
classDiagram
    direction TB

    class FrmLogin {
        -UsuarioBLL _usuarioBLL
        -SesionBLL _sesionBLL
        +btnIngresar_Click(sender, e) void
        -MostrarError(string mensaje) void
        -AbrirMenuPrincipal() void
    }

    class SesionBLL {
        -UsuarioBLL _usuarioBLL
        -PermisosBLL _permisosBLL
        +Login(string username, string password) ResultadoLogin
        +Logout() void
        -RegistrarIntento(Usuario u, bool exitoso, string motivo) void
    }

    class UsuarioBLL {
        -UsuarioRepository _repo
        -EncriptadorService _encriptador
        +BuscarPorUsername(string username) Usuario
        +ValidarCredencial(Usuario u, string password) bool
        +IncrementarIntentosFallidos(Usuario u) void
        +ResetearIntentos(Usuario u) void
        +Bloquear(Usuario u) void
    }

    class UsuarioRepository {
        -string GetConnectionString()
        +BuscarPorUsername(string username) Usuario
        +Actualizar(Usuario u) void
        +GuardarIntentoLogin(IntentoLogin i) void
    }

    class Usuario {
        +int Id
        +string Username
        +byte[] PasswordHash
        +byte[] Salt
        +string Nombre
        +string Email
        +int IntentosFallidos
        +bool Bloqueado
        +bool Activo
        +DateTime FechaUltimoLogin
        +List~Componente~ Permisos
        +ToString() string
    }

    class IntentoLogin {
        +long Id
        +int IdUsuario
        +string UsernameIngresado
        +DateTime FechaHora
        +bool Exitoso
        +string Motivo
    }

    class ResultadoLogin {
        +bool EsValido
        +MotivoRechazo Motivo
        +Usuario Usuario
    }

    class MotivoRechazo {
        <<enumeration>>
        Ninguno
        CredencialInvalida
        UsuarioBloqueado
        UsuarioInactivo
        SesionYaIniciada
    }

    class EncriptadorService {
        +Hash(string texto, byte[] salt) byte[]
        +GenerarSalt() byte[]
        +Verificar(string texto, byte[] salt, byte[] hash) bool
    }

    class SessionManager {
        <<Singleton>>
        +GetInstance()$ SessionManager
        +Login(Usuario u)$ void
        +Logout()$ void
        +IsInRole(TipoPermiso p) bool
    }

    class BitacoraBLL {
        +Registrar(string modulo, string criticidad, string descripcion) void
    }

    FrmLogin            ..>  SesionBLL       : usa
    SesionBLL           -->  UsuarioBLL
    SesionBLL           ..>  SessionManager  : Login(Usuario)
    SesionBLL           ..>  BitacoraBLL
    SesionBLL           ..>  ResultadoLogin  : crea
    UsuarioBLL          -->  UsuarioRepository
    UsuarioBLL          ..>  EncriptadorService
    UsuarioRepository   ..>  Usuario         : materializa
    UsuarioRepository   ..>  IntentoLogin
    ResultadoLogin      --> "0..1" Usuario
    ResultadoLogin      --> MotivoRechazo
    SessionManager      --> "0..1" Usuario   : sesión activa
```

## Responsabilidades

| Clase | Responsabilidad única |
|---|---|
| `FrmLogin` | Capturar credenciales y mostrar resultados. Nada de lógica. |
| `SesionBLL` | Orquestar el login: validar, bloquear, auditar, abrir sesión. |
| `UsuarioBLL` | Reglas del usuario (credencial, intentos, bloqueo). |
| `UsuarioRepository` | Único punto que habla SQL. |
| `EncriptadorService` | Hash + salt. La contraseña en claro nunca sale de acá. |
| `SessionManager` | Instancia única de sesión (ver [DC-sesion-singleton.md](DC-sesion-singleton.md)). |

> `SesionBLL` devuelve un `ResultadoLogin` en vez de lanzar excepciones para los
> casos de negocio esperables (credencial inválida, usuario bloqueado): una
> credencial mal tipeada no es un caso excepcional.
