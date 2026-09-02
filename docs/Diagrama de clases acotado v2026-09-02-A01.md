# Diagrama de clases acotado — Módulo Login (v2026-09-02-A01)

Versión recortada, con el mismo alcance que el diagrama original adjunto por
el usuario (solo el módulo de **Login**), actualizada a los nombres y la
arquitectura actuales del código en `ingSoftWinForm`. Para el diagrama
completo de las 5 capas ver
[Diagrama de clases v2026-09-02-A01.md](Diagrama%20de%20clases%20v2026-09-02-A01.md).

## Equivalencia con el diagrama original

| Diagrama original (Login) | Equivalente actual |
|---|---|
| `GUI.Form1` | `UI.Login.FrmLogin` |
| `BLL.GestionUsuarios` | `BLL.SessionBLL` |
| `BE.USUARIO` | `BE.Entity.User` |
| `Servicios.SessionManager` | `Services.SessionManager` (mismo Singleton) |
| `Servicios.CryptoManager` | `Services.HashManager` |
| `DAL.Mapper_usuario<USUARIO>` | `BE.Mapper.UserMapper` |
| `DAL.Mapper<T>` | `BE.Base.BaseMapper<TEntity>` |
| *(no existía)* | `DAL.IUserDAL` / `DAL.UserDAL` / `DAL.DatabaseHelper` (hoy la BLL ya no habla directo con el mapper, pasa por una interfaz de repositorio) y `BE.Entity.LoginResult` (el método `Login` ahora devuelve un resultado tipado en vez de `void`) |

## Diagrama

```plantuml
@startuml DiagramaDeClases_Login_Acotado

skinparam classAttributeIconSize 0
skinparam classFontStyle bold
skinparam shadowing false
skinparam roundcorner 6
skinparam class {
    BackgroundColor #FDEBD8
    BorderColor #C87F41
    ArrowColor #333333
    FontColor #222222
}
skinparam package {
    BackgroundColor #FFFFFF
    BorderColor #999999
}

namespace UI.Login {
    class FrmLogin {
        - sessionBLL : BLL.SessionBLL
        + FrmLogin()
        - BtnAceptar_Click(sender : object, e : EventArgs) : void
        - BtnCancelar_Click(sender : object, e : EventArgs) : void
        - MostrarError(mensaje : string) : void
    }
}

namespace BLL {
    class SessionBLL {
        - {static} MaxFailedAttempts : int
        - userDAL : DAL.IUserDAL
        + IsLoggedIn : bool
        + CurrentUser : BE.Entity.User
        + SessionBLL()
        + SessionBLL(userDAL : DAL.IUserDAL)
        + Login(username : string, password : string) : BE.Entity.LoginResult
        + Logout() : void
        - RegistrarFallo(user : BE.Entity.User) : BE.Entity.LoginResult
    }
}

namespace DAL {
    interface IUserDAL {
        + GetByUsername(username : string) : BE.Entity.User
        + Block(username : string) : bool
        + IncrementFailedAttempts(username : string) : void
        + ResetFailedAttempts(id : Guid) : void
    }

    class UserDAL {
        - dbHelper : DatabaseHelper
        - mapper : BE.Mapper.UserMapper
        + UserDAL()
        + GetByUsername(username : string) : BE.Entity.User
        + Block(username : string) : bool
        + IncrementFailedAttempts(username : string) : void
        + ResetFailedAttempts(id : Guid) : void
    }

    class DatabaseHelper {
        - connectionString : string
        + DatabaseHelper()
        + ExecuteDataSet(query, commandType, parameters) : DataSet
        + ExecuteNonQuery(query, commandType, parameters) : int
        + ExecuteScalar(query, commandType, parameters) : object
    }

    IUserDAL <|.. UserDAL
    UserDAL "1" o-- "1" DatabaseHelper
}

namespace BE.Base {
    abstract class "BaseMapper<TEntity>" as BaseMapper {
        + {abstract} MapToEntity(row : DataRow) : TEntity
        + MapToEntity(dataTable : DataTable) : TEntity
    }
}

namespace BE.Mapper {
    class UserMapper {
        + MapToEntity(row : DataRow) : BE.Entity.User
    }

    BE.Base.BaseMapper <|-- UserMapper
}

namespace BE.Entity {
    class User {
        + Username : string
        + PasswordHash : byte[]
        + Salt : byte[]
        + FirstName : string
        + LastName : string
        + Email : string
        + FailedAttempts : int
        + IsBlocked : bool
        + IsActive : bool
        + LastLoginAt : DateTime
        + ToString() : string
    }

    class LoginResult {
        + Status : BE.Enum.LoginStatus
        + User : User
        + IsValid : bool
        + {static} Ok(user : User) : LoginResult
        + {static} Fail(status : BE.Enum.LoginStatus) : LoginResult
    }

    LoginResult "1" o-- "0..1" User
}

namespace BE.Enum {
    enum LoginStatus {
        Success
        InvalidCredentials
        UserBlocked
        UserInactive
        SessionAlreadyOpen
    }
}

namespace Services {
    class SessionManager <<Singleton>> {
        - {static} _session : SessionManager
        + User : BE.Entity.User
        + StartedAt : DateTime
        - SessionManager()
        + {static} GetInstance : SessionManager
        + {static} IsLoggedIn() : bool
        + {static} Login(user : BE.Entity.User) : void
        + {static} Logout() : void
    }

    class HashManager <<static>> {
        + {static} GenerateSalt() : byte[]
        + {static} HashPassword(password : string, salt : byte[]) : byte[]
        + {static} VerifyPassword(password : string, salt : byte[], hash : byte[]) : bool
    }
}

' ===================== Composición / agregación (has-a, "1") =====================
UI.Login.FrmLogin  "1" o-- "1" BLL.SessionBLL
BLL.SessionBLL     "1" o-- "1" DAL.IUserDAL
DAL.UserDAL        "1" o-- "1" BE.Mapper.UserMapper
Services.SessionManager "1" o-- "0..1" BE.Entity.User

' ===================== Dependencias / uso («use», punteada) =====================
BLL.SessionBLL        ..> Services.SessionManager : «use»
BLL.SessionBLL        ..> Services.HashManager     : «use»
BLL.SessionBLL        ..> BE.Entity.LoginResult    : «use» (crea)
BE.Mapper.UserMapper  ..> BE.Entity.User           : «use» (instancia)

@enduml
```

## Notas de lectura

- Mismo lenguaje visual que el diagrama completo: rombo + "1" = agregación
  (campo propio), triángulo hueco continuo = herencia, triángulo hueco
  punteado = realización de interfaz, flecha punteada «use» = dependencia sin
  campo propio.
- Se dejaron afuera, a propósito, `BLL.BitacoraBLL`, `BE.Entity.Bitacora`,
  `Services.BitacoraManager` y las pantallas `FrmMain`/`FrmProfile`/`FrmEvent`
  — no participan del caso de uso Login. Están documentadas en el diagrama
  completo.
- A diferencia del diagrama original, hoy `BLL.SessionBLL` no habla
  directamente con el mapper: pasa por `DAL.IUserDAL` (inyectable, ver los
  dos constructores), y es `DAL.UserDAL` quien internamente usa
  `DatabaseHelper` + `BE.Mapper.UserMapper`. Es la diferencia arquitectónica
  más relevante entre la versión vieja del proyecto y la actual.
