# ingSoftWinForm

Aplicación WinForms (.NET 8) organizada en capas. Implementa el CU-01 *Iniciar
sesión* (T02), la bitácora de auditoría (T06a), el hash de contraseñas (T03) y la
**gestión de perfiles de usuario con roles y permisos (T04, patrón Composite)**.

Este documento tiene un **[Quick start](#quick-start)** para levantar todo desde cero y
después explica **la base de datos**, **cómo se crean los usuarios**, los **diagramas
de secuencia de login y logout**, el **SessionManager**, la **bitácora de auditoría**,
los **formularios MDI** y la **gestión de perfiles (roles y permisos)**.

---

## Quick start

Todos los comandos se corren desde la **raíz del repositorio** (`uai-ing-soft/`), los
`.sh` desde **Git Bash**.

### Requisitos

| Herramienta | Para qué | Cómo comprobarla |
|---|---|---|
| .NET SDK 8 o superior | compilar y correr | `dotnet --version` |
| SQL Server Express (instancia `SQLEXPRESS`) | la base `IF_DB` | `sqlcmd -S localhost\SQLEXPRESS -E -C -Q "SELECT @@VERSION"` |
| `sqlcmd` en el PATH | ejecutar los scripts | `sqlcmd -?` |
| `python` en el PATH | calcular el hash PBKDF2 en `create-user.sh` | `python --version` |

Si `sqlcmd` no está en el PATH, suele estar en
`C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn`.

### 1. Levantar SQL Server

El motor corre como un servicio de Windows. Si no está iniciado (PowerShell **como administrador**):

```powershell
Get-Service 'MSSQL$SQLEXPRESS'            # ver el estado
Start-Service 'MSSQL$SQLEXPRESS'          # iniciarlo
```

Comprobar que responde:

```bash
sqlcmd -S localhost\SQLEXPRESS -E -C -Q "SELECT @@SERVERNAME"
```

> Si tu instancia tiene otro nombre, cambiá el `Server=` de la cadena de conexión en
> `01 - Presentation Layer/UI/App.config` y pasá `-S '<servidor>'` a los scripts de `sql/`.

### 2. Crear la base y las tablas (solo las que falten)

```bash
./sql/init-db.sh
```

`sql/init-db.sh` crea `IF_DB` si no existe y después las tablas que falten: `[User]`,
`[Bitacora]` y las de roles y permisos (`[Permission]`, `[Permission_Permission]`,
`[Role]`, `[Role_Permission]`, `[User_Role]`, con el catálogo sembrado). Es **seguro de
re-ejecutar y no borra datos**: si `[User]` ya existe no corre `01_create_table_User.sql`
(que hace `DROP TABLE`). Al final muestra cuántas filas tiene cada tabla.

### 3. Crear el primer usuario (administrador)

```bash
./sql/create-user.sh -u admin -p Admin123 -f Admin -l "Del Sistema" -e admin@if.local
```

El usuario creado queda con el rol **`administrador`** por defecto, que es el único que
puede abrir *Gestión de roles*. Salida esperada:

```text
Creando usuario 'admin' en localhost\SQLEXPRESS / IF_DB ...
OK. Usuario 'admin' creado (rol: administrador).

Username FirstName LastName       FailedAttempts IsBlocked IsActive
-------- --------- -------------- -------------- --------- --------
admin    Admin     Del Sistema    0              0         1
```

Variantes con el flag `-r`:

```bash
# Moderador: puede ver la bitácora, pero no gestionar roles
./sql/create-user.sh -u moderadora -p Mod12345 -f Ana -l Lopez -r moderador

# Cliente: Mi perfil, Cerrar sesión e Inicio
./sql/create-user.sh -u cliente1 -p Cli12345 -r client

# Sin rol (no podrá hacer nada hasta que se le asigne uno)
./sql/create-user.sh -u sinrol -p Test1234 -r ninguno
```

El rol se valida **antes** de insertar: si no existe (o si faltan las tablas de roles, es
decir, no se corrió el paso 2) el script corta con un mensaje claro y no deja un usuario
creado a medias. Roles disponibles: `invitado`, `client`, `moderador`, `administrador`.

### 4. Compilar y ejecutar

```bash
dotnet build ingSoftWinForm/ingSoftWinForm.sln
dotnet run --project "ingSoftWinForm/01 - Presentation Layer/UI/GUI.csproj"
```

Iniciá sesión con `admin` / `Admin123`. Cualquier otra persona puede crearse una cuenta
con el botón **Registrarse** del login: queda con el rol `invitado` (solo *Mi perfil* y
*Cerrar sesión*).

### 5. Darle permisos a un usuario ya registrado

Un usuario que se registró desde la app tiene rol `invitado`. Para que pueda gestionar
perfiles hay que darle el rol `administrador`; esto **no puede hacerse desde la app**
si todavía no hay ningún administrador, por eso existe el script:

```bash
./sql/set-user-role.sh -u pepe                      # lo hace administrador
./sql/set-user-role.sh -u pepe -r moderador         # le da otro rol
./sql/set-user-role.sh -u pepe -r administrador -x  # le quita ese rol
```

El script es idempotente y al terminar lista los roles actuales del usuario. El usuario
ve el cambio **en su próximo inicio de sesión**. Una vez que hay un administrador, el
resto se gestiona desde la app (*Sesión ▸ Gestión de roles*).

### 6. Tests

```bash
dotnet test "ingSoftWinForm/06 - Tests/Tests/Tests.csproj"
```

### Resumen de scripts de `sql/`

| Script | Qué hace | ¿Destructivo? |
|---|---|---|
| `init-db.sh` | Crea la base y las tablas que falten | No |
| `create-user.sh` | Crea un usuario (hash PBKDF2) y le asigna un rol (default `administrador`) | No |
| `set-user-role.sh` | Asigna o quita un rol a un usuario existente | No |
| `01_create_table_User.sql` | Crea `[User]` | **Sí** (`DROP TABLE`) |
| `02_seed_User.sql` | Solo lista usuarios y documenta el alta manual | No |
| `03_create_table_Bitacora.sql` | Crea `[Bitacora]` si falta | No |
| `04_init_create_roles_permission.sql` | Crea las tablas de roles/permisos si faltan y siembra el catálogo | No (idempotente) |

---

## 1. Arquitectura en capas

```
01 - Presentation Layer/UI      →  GUI.csproj        (net8.0-windows)  FrmLogin, FrmRegister, FrmMain, FrmLogout, FrmProfile, FrmEvent, FrmInicio, FrmRoleManagement
02 - Service Layer/Services     →  Services.csproj   (net8.0)          HashManager, SessionManager, BitacoraManager
03 - Business Logic Layer/BLL   →  BLL.csproj        (net8.0)          SessionBLL, UserBLL, RoleBLL, BitacoraBLL
04 - Business Entity/BE         →  BE.csproj         (net8.0)          User, Role, Permission (Composite), Bitacora, LoginResult, OperationResult, enums, mappers, bases
05 - Data Access Layer/DAL      →  DAL.csproj        (net8.0)          DatabaseHelper, IUserDAL/UserDAL, IRoleDAL/RoleDAL, IPermissionDAL/PermissionDAL, IBitacoraDAL/BitacoraDAL
06 - Tests/Tests                →  Tests.csproj      (net8.0)          xUnit: Composite, permisos, RoleBLL, UserBLL (con DAL en memoria)
```

Dependencias entre proyectos (`ProjectReference`):

```mermaid
graph LR
    GUI --> BLL
    GUI --> Services
    GUI --> BE
    BLL --> DAL
    BLL --> Services
    BLL --> BE
    DAL --> BE
    DAL --> Services
    Services --> BE
```

La UI **nunca** referencia a DAL: todo pasa por `SessionBLL`. Y la UI tampoco llama
al `SessionManager` directamente — usa `SessionBLL.CurrentUser` / `SessionBLL.Logout()`.

Paquetes NuGet: `Microsoft.Data.SqlClient` y `System.Configuration.ConfigurationManager`,
ambos solo en DAL.

---

## 2. Base de datos

### Motor y conexión

- **Motor:** SQL Server (probado sobre `localhost\SQLEXPRESS`).
- **Base:** `IF_DB`.
- **Cadena de conexión:** `01 - Presentation Layer/UI/App.config`, con el nombre `IF_DB`:

```xml
<add name="IF_DB"
     connectionString="Server=localhost\SQLEXPRESS;Database=IF_DB;Integrated Security=True;TrustServerCertificate=True;"
     providerName="Microsoft.Data.SqlClient" />
```

`DAL.DatabaseHelper` es el **único punto del sistema que abre conexiones**. Lee esa
entrada con `ConfigurationManager` y lanza `ConfigurationErrorsException` si no existe.
Expone `ExecuteDataSet(...)`, `ExecuteNonQuery(...)`, `ExecuteScalar(...)` y
`ExecuteTransaction(...)` (varias sentencias en una transacción: todas o ninguna), todos
con `SqlParameter[]` (nunca concatenación de strings → sin SQL injection).

### Tabla `[dbo].[User]`

Script: `../sql/01_create_table_User.sql`.

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | `UNIQUEIDENTIFIER` | PK clustered, default `NEWID()` |
| `Username` | `NVARCHAR(50)` | `UNIQUE` (`UQ_User_Username`) |
| `PasswordHash` | `VARBINARY(32)` | PBKDF2-SHA256, 32 bytes |
| `Salt` | `VARBINARY(16)` | salt aleatorio por usuario |
| `FirstName` / `LastName` | `NVARCHAR(100)` | |
| `Email` | `NVARCHAR(150)` NULL | único entre los no-NULL (índice filtrado `UX_User_Email`) |
| `FailedAttempts` | `INT` | default 0 |
| `IsBlocked` | `BIT` | default 0 |
| `IsActive` | `BIT` | default 1 |
| `LastLoginAt` | `DATETIME2` NULL | |
| `CreatedAt` / `CreatedBy` / `UpdatedAt` / `UpdatedBy` | | auditoría, refleja `BE.Base.BaseAuditEntity` |

Dos detalles que hacen ruido si no se conocen:

- `[User]` es palabra reservada en T-SQL → **siempre entre corchetes**.
- `UX_User_Email` es un **índice filtrado**, así que todo `INSERT`/`UPDATE` sobre la
  tabla exige `QUOTED_IDENTIFIER ON`. Desde la app y desde SSMS ya viene en ON; desde
  `sqlcmd` hay que pasar el flag `-I` o falla con el error 1934.

### Consultas que ejecuta la app

Todas en `05 - Data Access Layer/DAL/UserDAL.cs`, texto plano parametrizado (no hay
stored procedures):

| Método | SQL |
|---|---|
| `GetByUsername(username)` | `SELECT * FROM [User] WHERE Username = @Username` |
| `GetAll()` | `SELECT * FROM [User]` |
| `Block(username)` | `UPDATE [User] SET IsBlocked = 1, UpdatedAt = SYSDATETIME() ...` |
| `IncrementFailedAttempts(username)` | `UPDATE [User] SET FailedAttempts = FailedAttempts + 1 ...` |
| `ResetFailedAttempts(id)` | `UPDATE [User] SET FailedAttempts = 0, LastLoginAt = SYSDATETIME() ...` |
| `EmailExists(email)` | `SELECT COUNT(1) FROM [User] WHERE Email = @Email` |
| `Insert(user, roleName)` | `INSERT [User]` + `INSERT [User_Role]` en **una transacción** (si el rol no existe, rollback) |

Notar que **la contraseña nunca viaja al SQL**: se busca solo por `Username` y la
verificación del hash la hace la BLL en memoria con `HashManager`.

### Tablas de roles y permisos (T04)

Script: `../sql/04_init_create_roles_permission.sql`. Detalle y diagramas en la sección
[10](#10-gestión-de-perfiles-roles-y-permisos-t04).

| Tabla | Para qué |
|---|---|
| `[Permission]` | Catálogo: permisos simples y compuestos (`IsCompound`), con `Code` único |
| `[Permission_Permission]` | Árbol del Composite: `ParentId` (compuesto) → `ChildId` (simple u otro compuesto) |
| `[Role]` | Roles (se gestionan desde la app) |
| `[Role_Permission]` | Permisos que otorga cada rol (cae en cascada al borrar el rol) |
| `[User_Role]` | Roles de cada usuario (N:M; FK a `[User].Id`) |

### Puesta en marcha

> **Atajo:** `./sql/init-db.sh` hace todo lo de abajo en un paso y solo crea lo que
> falta (ver el [Quick start](#quick-start)). Los comandos manuales quedan como referencia.

Crear la base:

```bash
sqlcmd -S localhost\SQLEXPRESS -E -C -Q "IF DB_ID('IF_DB') IS NULL CREATE DATABASE [IF_DB];"
```

Crear la tabla:

```bash
sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql/01_create_table_User.sql
```

> `01_create_table_User.sql` empieza con un `DROP TABLE` condicional: volver a correrlo
> borra todos los usuarios cargados.

Crear la tabla de bitácora:

```bash
sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql/03_create_table_Bitacora.sql
```

> Este, al revés, **no** dropea nada (`IF OBJECT_ID(...) IS NULL`): reejecutarlo no
> puede borrar el historial de auditoría.

Crear las tablas de roles y permisos y sembrar el catálogo (requiere que exista `[User]`):

```bash
sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql/04_init_create_roles_permission.sql
```

> Idempotente: no dropea nada y no duplica filas al reejecutarlo.

---

## 3. Cómo se crean los usuarios

**No se pueden sembrar desde un `.sql` suelto.** La contraseña se guarda como
PBKDF2-SHA256 con 100.000 iteraciones (`Services.HashManager`) y T-SQL no tiene PBKDF2
— `HASHBYTES` hace un SHA2_256 de una sola pasada, que no es lo mismo. Un hash escrito
a mano hace que `HashManager.VerifyPassword` devuelva siempre `false` y el login no
entre nunca.

Por eso existe `../sql/create-user.sh`, que calcula el mismo par salt/hash (vía
`python`) y hace el `INSERT`:

```bash
./sql/create-user.sh -u admin -p Admin123 -f Admin -l "Del Sistema" -e admin@if.local
```

Opciones:

| Flag | Significado | Default |
|---|---|---|
| `-u` | username (obligatorio, único) | — |
| `-p` | contraseña en claro (obligatorio) | — |
| `-f` | FirstName | `Nombre` |
| `-l` | LastName | `Apellido` |
| `-e` | Email | `NULL` |
| `-r` | Rol a asignar (T04); `ninguno` = sin rol | `administrador` |
| `-S` | instancia SQL | `localhost\SQLEXPRESS` |
| `-d` | base de datos | `IF_DB` |
| `-h` | ayuda | — |

Requisitos: `python` y `sqlcmd` en el PATH. La contraseña se pasa al intérprete por
variable de entorno y no por argumento, porque los argumentos de un proceso son
visibles para cualquier otro proceso de la máquina. El script corta con `RAISERROR` si
el username ya existe o si el rol indicado no existe, y al terminar lista los usuarios
cargados.

**Otras formas de crear o modificar usuarios:**

- **Desde la app:** el botón *Registrarse* del login (`FrmRegister` → `UserBLL.Register`)
  crea un usuario con hash PBKDF2 y rol `invitado`. Valida: usuario de 3 a 50 caracteres
  sin espacios, contraseña de al menos 8 y repetida igual, nombre y apellido obligatorios,
  email opcional con formato válido, y que usuario y email no estén repetidos.
- **Darle o quitarle un rol a un usuario existente** (por ejemplo hacer administrador a
  alguien que se registró): `./sql/set-user-role.sh -u pepe [-r rol] [-x]`.

Para ver qué hay en la tabla sin crear nada:

```bash
sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql/02_seed_User.sql
```

`02_seed_User.sql` **no crea usuarios**: solo hace un `SELECT` y documenta el
procedimiento manual, con el `INSERT` comentado y el snippet de Python para generar el
par salt/hash a mano.

### Cómo se guarda la contraseña — `Services.HashManager`

| Parámetro | Valor |
|---|---|
| Algoritmo | PBKDF2 (`Rfc2898DeriveBytes.Pbkdf2`) |
| Hash interno | SHA256 |
| Iteraciones | 100.000 |
| Salt | 16 bytes aleatorios por usuario |
| Hash | 32 bytes |

`VerifyPassword` compara con `CryptographicOperations.FixedTimeEquals`: no corta en el
primer byte distinto, así el tiempo de respuesta no filtra información del hash
(RNF-Seguridad-01). La contraseña en claro nunca sale de esta clase.

---

## 4. Login

### Reglas de negocio (`03 - Business Logic Layer/BLL/SessionBLL.cs`)

1. Usuario inexistente y contraseña incorrecta devuelven **el mismo**
   `InvalidCredentials`: el mensaje no debe revelar si el usuario existe
   (CU-01, FA-1 paso 4c).
2. `IsActive = 0` → `UserInactive`. `IsBlocked = 1` → `UserBlocked`.
3. **Bloqueo automático a los 3 intentos fallidos** (`MaxFailedAttempts = 3`,
   RNF-Seguridad-02).
4. Login correcto → resetea `FailedAttempts` y sella `LastLoginAt`.
5. Si ya había una sesión abierta → `SessionAlreadyOpen`, no se abre una segunda (FA-4).
6. **T04:** con la contraseña correcta, antes de abrir la sesión se cargan los roles del
   usuario con su árbol de permisos (`IRoleDAL.GetByUser` → `user.Roles`). Así la
   bitácora del login y el menú ya conocen los permisos.

El resultado se devuelve como `LoginResult` y no con excepciones: una credencial mal
tipeada es un caso de negocio esperable, no excepcional. `LoginStatus` = `Success`,
`InvalidCredentials`, `UserBlocked`, `UserInactive`, `SessionAlreadyOpen`.

### Diagrama de secuencia — Login

```mermaid
sequenceDiagram
    actor U as Usuario
    participant FL as FrmLogin
    participant BLL as SessionBLL
    participant DAL as UserDAL
    participant H as HashManager
    participant S as SessionManager

    U->>FL: username / password + Aceptar
    FL->>+BLL: Login(username, password)

    BLL->>+DAL: GetByUsername(username)
    DAL-->>-BLL: User? (UserMapper.MapToEntity)

    alt user == null
        BLL-->>FL: Fail(InvalidCredentials)
    else user.IsActive == false
        BLL-->>FL: Fail(UserInactive)
    else user.IsBlocked
        BLL-->>FL: Fail(UserBlocked)
    else usuario habilitado
        Note over BLL: tras validar la contraseña (T04) carga user.Roles<br/>con IRoleDAL.GetByUser antes de SessionManager.Login
        BLL->>+H: VerifyPassword(password, user.Salt, user.PasswordHash)
        H-->>-BLL: bool

        alt contrasena incorrecta
            BLL->>DAL: IncrementFailedAttempts(username)
            alt intentos >= 3
                BLL->>DAL: Block(username)
                BLL-->>FL: Fail(UserBlocked)
            else
                BLL-->>FL: Fail(InvalidCredentials)
            end
        else contrasena correcta
            BLL->>DAL: ResetFailedAttempts(user.Id)
            BLL->>+S: SessionManager.Login(user)
            alt ya habia sesion abierta
                S-)BLL: InvalidOperationException
                BLL-->>FL: Fail(SessionAlreadyOpen)
            else
                S-->>-BLL: sesion creada
                BLL-->>FL: Ok(user)
            end
        end
    end
    deactivate BLL
```

Si la base no responde, `FrmLogin` captura la excepción y muestra *"No se pudo conectar
con el servidor. Intente nuevamente."* (FA-5), con el detalle en `Debug.WriteLine`.

> Nota: el `User` que queda en sesión es el snapshot leído **antes** del
> `ResetFailedAttempts`, así que el `LastLoginAt` que muestra el perfil es el del acceso
> anterior, no el de la sesión en curso. Es lo que normalmente se espera de un "último
> acceso", pero conviene saberlo.

---

## 5. Logout

Cerrar sesión no toca la base: solo descarta el singleton en memoria y cierra el MDI.

```mermaid
sequenceDiagram
    actor U as Usuario
    participant FM as FrmMain MDI
    participant FLO as FrmLogout
    participant BLL as SessionBLL
    participant S as SessionManager

    U->>FM: Menu Sesion > Cerrar sesion
    FM->>FLO: ShowDialog(this)
    FLO-->>U: pide confirmacion

    alt Cancelar
        FLO-->>FM: DialogResult.Cancel
        FM-->>U: sigue en el MDI
    else Aceptar
        U->>FLO: Aceptar
        FLO->>+BLL: Logout()
        BLL->>+S: SessionManager.Logout()
        S->>S: _session = null
        S-->>-BLL: ok
        BLL-->>-FLO: ok
        FLO-->>FM: DialogResult.OK
        FM->>FM: Close()
        Note over FM: cerrada la sesion no queda nada operable:<br/>se cierra el MDI y con el la aplicacion
    end
```

`FrmLogout` no llama al `SessionManager`: pasa por `SessionBLL.Logout()`, que respeta el
corte de capas.

---

## 6. SessionManager (Singleton)

`02 - Service Layer/Services/SessionManager.cs`. Existe **una y sólo una** sesión activa
mientras la aplicación está en ejecución (ver `DC-sesion-singleton.md` en `docs/`).

```csharp
private static SessionManager? _session;   // el estado vive en un campo static
private SessionManager() { }               // constructor privado: nadie la instancia desde afuera

public User User { get; private set; }     // usuario logueado, solo lectura desde afuera
public DateTime StartedAt { get; private set; }
```

| Miembro | Qué hace |
|---|---|
| `SessionManager.Login(user)` | Crea la instancia con `User` y `StartedAt = DateTime.Now`. Lanza `InvalidOperationException` si **ya** hay sesión. |
| `SessionManager.Logout()` | Pone `_session = null`. Lanza `InvalidOperationException` si **no** hay sesión. |
| `SessionManager.IsLoggedIn()` | `_session != null`. Es el único chequeo que siempre es seguro llamar. |
| `SessionManager.GetInstance` | Devuelve la instancia. Lanza `InvalidOperationException` si no hay sesión. |
| `SessionManager.HasPermission(code)` | (T04) ¿el usuario en sesión tiene el permiso? Recorre recursivamente el Composite de sus roles. Sin sesión devuelve `false`. |

Es un singleton **con ciclo de vida**, no el clásico "instancia perezosa eterna": se
crea en el login y se destruye en el logout. Por eso `GetInstance` puede tirar excepción
y hay que preguntar `IsLoggedIn()` antes.

**Cómo lo consume el resto del sistema:**

- La UI **no** lo toca directamente. Pasa por `SessionBLL`:
  - `sessionBLL.CurrentUser` → `IsLoggedIn() ? GetInstance.User : null` (devuelve `null`
    en vez de romper).
  - `sessionBLL.IsLoggedIn`
  - `sessionBLL.HasPermission(code)` (T04, lo usa `FrmMain` para armar el menú)
  - `sessionBLL.Logout()`
- `SessionBLL.Login` envuelve `SessionManager.Login` en un try/catch y traduce la
  excepción a `LoginStatus.SessionAlreadyOpen` (FA-4).

Consecuencias a tener presentes:

- Cada `Form` crea su propio `new SessionBLL()`, pero todos ven la **misma** sesión: el
  estado es `static`, no de instancia.
- El `User` guardado es un snapshot del momento del login; si cambian los datos en la
  base, la sesión no se entera hasta el próximo login.
- No hay sincronización de hilos ni `Lazy<T>`. Alcanza para WinForms, donde todo corre
  en el hilo de UI.

---

## 7. Bitácora de auditoría

**RNF-Seguridad-03 del CU-01: todo intento de acceso queda auditado.** La bitácora es
una tabla append-only: se inserta y no se modifica nunca.

### Tabla `[dbo].[Bitacora]`

Script: `../sql/03_create_table_Bitacora.sql`. Refleja `BE.Entity.Bitacora`.

| Columna | Tipo | Notas |
|---|---|---|
| `id_bitacora` | `INT IDENTITY` | PK clustered. **Único nombre del modelo que no está en inglés**, por pedido del diagrama de clases |
| `Type` | `NVARCHAR(20)` | enum `BitacoraType`: `Event` / `Error` |
| `NameEvent` | `NVARCHAR(30)` | enum `NameEvent`: `Login`, `Logout`, `CrearUsuario`, ... |
| `Priority` | `NVARCHAR(20)` | enum `Priority`: `Low` / `Medium` / `High` / `Critical` / `Fatal` |
| `Detail` | `NVARCHAR(500)` | texto libre; `BitacoraManager` lo recorta a 500 |
| `BitacoraDate` | `DATETIME2` | default `SYSDATETIME()` |
| `IdUser` / `Email` / `FirstName` / `LastName` | `NVARCHAR` | foto del usuario, todos `string` |
| `RolesPermisos` | `NVARCHAR(MAX)` | roles y permisos que tenía en ese momento |

Tres decisiones que conviene tener presentes:

- **Los enums se guardan por nombre, no por ordinal.** Una bitácora se consulta con un
  `SELECT` suelto: `'Critical'` se lee, `4` no.
- **No hay FK contra `[User]`.** Los datos del usuario se *copian*, no se referencian: la
  traza tiene que sobrevivir a una baja o un renombre, y además hay filas sin usuario
  (un login con un username que no existe).
- **`RolesPermisos` se completa con los roles y permisos del usuario** en ese momento
  (T04), por ejemplo `Roles: client | Permisos: VER_MI_PERFIL, CERRAR_SESION, VER_INICIO`.
  Lo arma `BitacoraManager.AplanarRolesPermisos` aplanando el Composite con
  `Permission.Flatten()` (recursivo). Queda vacío si el usuario no tiene roles.

### Quién hace qué

```
Services.BitacoraManager   construye la entidad (EventoBitacora / ErrorBitacora)
BLL.BitacoraBLL            la persiste, y consulta (GetAll / GetByFilter)
DAL.BitacoraDAL            el INSERT y los SELECT
UI.Event.FrmEvent          el visor de solo lectura
```

Ojo con el nombre del visor: **sólo la capa de UI habla de `Event`**
(`UI.Event.FrmEvent`). La entidad, la BLL, la DAL y la tabla se siguen llamando
`Bitacora`. Y como el namespace `UI.Event` tapa al tipo `BE.Entity.Bitacora`,
`FrmEvent.cs` lo nombra por un alias (`using BitacoraEntity = BE.Entity.Bitacora;`).

`BitacoraManager` es la pieza `Servicios.Bitacora` del diagrama de clases. Solo
**construye** la entidad y no la persiste, porque la capa de servicios no referencia a
DAL. Se llama `BitacoraManager` y no `Bitacora` para no chocar con `BE.Entity.Bitacora`,
y para acompañar a `SessionManager` y `HashManager`.

Cada fábrica tiene dos sobrecargas:

- con `User` explícito — la que usa el login, porque en un intento fallido **todavía no
  hay sesión abierta** y `SessionManager.GetInstance` tiraría excepción;
- sin `User` — toma el de la sesión activa, y si no hay ninguna deja los campos vacíos.

### La auditoría nunca voltea la operación auditada

`BitacoraBLL.Registrar` atrapa la excepción y la manda a `Debug.WriteLine`. Si se cae la
base, el login tiene que poder seguir devolviendo su `LoginResult` (CU-01, FA-5): un
fallo al auditar no puede convertirse en un crash.

El visor va al revés: ahí el error **sí** se avisa con un `MessageBox`, porque si no se
puede leer no hay nada que mostrar en pantalla.

### Qué registra el login

Todo esto sale de `SessionBLL` (`03 - Business Logic Layer/BLL/SessionBLL.cs`):

| Situación | Type | NameEvent | Priority |
|---|---|---|---|
| Username inexistente | `Error` | `Login` | `Medium` |
| Usuario dado de baja | `Error` | `Login` | `Medium` |
| Usuario bloqueado | `Error` | `Login` | `High` |
| Credencial inválida | `Error` | `Login` | `Medium` |
| Bloqueo automático al 3º intento | `Error` | `Login` | `Critical` |
| FA-4: ya había una sesión abierta | `Error` | `Login` | `High` |
| Inicio de sesión exitoso | `Event` | `Login` | `Low` |
| Cierre de sesión | `Event` | `Logout` | `Low` |

### Qué registra la gestión de perfiles (T04)

| Situación | Type | NameEvent | Priority |
|---|---|---|---|
| Registro de un usuario | `Event` | `CrearUsuario` | `Low` |
| Crear / eliminar un rol | `Event` | `CrearRol` / `EliminarRol` | `Medium` |
| Agregar / quitar un permiso a un rol | `Event` | `AsignarPermisoRol` / `QuitarPermisoRol` | `Medium` |
| Asignar / quitar un rol a un usuario | `Event` | `AsignarRolUsuario` / `QuitarRolUsuario` | `Medium` |
| Operación sin el permiso requerido | `Error` | `AccesoNoAutorizado` | `High` |
| Falla de base de datos en cualquiera de las anteriores | `Error` | `ErrorSistema` | `High` |

Los valores nuevos de `NameEvent` (9 a 14) se agregaron al final del enum sin tocar los
ordinales existentes.

Dos cuidados en el código del login:

- en `Logout()` el usuario se toma **antes** de `SessionManager.Logout()`, que borra la sesión;
- el caso "username inexistente" no tiene `User`: el username tipeado va en el `Detail`.
  **La contraseña no se registra nunca.**

### Verla — `UI.Event.FrmEvent`

Menú *Sesión ▸ Bitácora*. Grilla de sólo lectura con cuatro filtros:

| Filtro | Control | Vacío significa |
|---|---|---|
| Rango de fechas | `dtpDesde` / `dtpHasta` | siempre se aplica; el “hasta” es inclusivo por día |
| Tipo | `cboTipo` | `(todos)` → no filtra |
| Evento | `cboEvento` | `(todos)` → no filtra |
| Prioridad | `cboPrioridad` | `(todos)` → no filtra |

Los tres combos se llenan con `System.Enum.GetValues<TEnum>()`, y guardan los valores
del enum **en crudo** — no su texto — así que se recuperan tipados sin volver a parsear.

El filtrado se resuelve en el motor, no en memoria: la bitácora crece sin techo y
traerla entera para descartar en el cliente no escala. `BitacoraDAL.GetByFilter` usa el
patrón `(@P IS NULL OR col = @P)` para cada enum opcional, que evita armar el `WHERE`
concatenando texto:

```sql
WHERE [BitacoraDate] >= @From
  AND [BitacoraDate] <  @To
  AND (@Type      IS NULL OR [Type]      = @Type)
  AND (@NameEvent IS NULL OR [NameEvent] = @NameEvent)
  AND (@Priority  IS NULL OR [Priority]  = @Priority)
```

También se puede consultar directo por SQL:

```bash
sqlcmd -S localhost\SQLEXPRESS -E -C -I -W -d IF_DB -Q "SET NOCOUNT ON; SELECT id_bitacora, Type, NameEvent, Priority, BitacoraDate, FirstName, LastName, Detail FROM [dbo].[Bitacora] ORDER BY id_bitacora DESC;"
```

---

## 8. Formularios MDI

### Quién es contenedor y quién no

| Formulario | Rol |
|---|---|
| `FrmLogin` | Diálogo **modal**, antes del MDI. Lo abre `Program.Main` con `ShowDialog()`. |
| `FrmRegister` | Diálogo **modal** abierto desde `FrmLogin` (botón *Registrarse*). |
| `FrmMain` | **Contenedor MDI** (`IsMdiContainer = true`). Es el `Application.Run(...)`. |
| `FrmProfile`, `FrmEvent`, `FrmInicio`, `FrmRoleManagement` | **Ventanas hijas** MDI. |
| `FrmLogout` | Diálogo **modal** sobre el MDI (`ShowDialog(this)`), no es hijo. |

Login y logout son modales a propósito: un formulario modal no puede ser hijo MDI, y
además son decisiones que deben bloquear al resto de la aplicación.

### Arranque

```csharp
// Program.cs — CU-01: sin sesión iniciada no se entra al menú principal.
using var login = new FrmLogin();
if (login.ShowDialog() != DialogResult.OK) return;
Application.Run(new FrmMain());
```

Si el login se cancela o falla, `Main` retorna y la aplicación **nunca** llega a crear
el MDI.

### Apertura de hijos: `AbrirHijo<TForm>()`

```csharp
public void AbrirHijo<TForm>() where TForm : Form, new()
{
    var abierto = MdiChildren.OfType<TForm>().FirstOrDefault();
    if (abierto != null)
    {
        if (abierto.WindowState == FormWindowState.Minimized)
            abierto.WindowState = FormWindowState.Normal;
        abierto.Activate();
        return;
    }
    var frm = new TForm { MdiParent = this };
    frm.Show();
}
```

Es genérico y con restricción `new()`, así que agregar una pantalla nueva es
`AbrirHijo<FrmLoQueSea>()` desde el menú, sin escribir código de manejo de ventanas.
La regla es **una instancia por tipo**: si ya está abierta se restaura (por si estaba
minimizada) y se activa, en vez de duplicarla.

`FrmProfile` se abre en `FrmMain_Load` (si el usuario tiene `VER_MI_PERFIL`) y **no** en
el constructor: en el constructor el contenedor MDI todavía no tiene el handle creado y
asignar `MdiParent` falla. Es la primera pantalla que ve el usuario al entrar.

### Menú

`menuStrip.MdiWindowListItem = mnuVentana` → WinForms mantiene solo la lista de ventanas
abiertas dentro del menú *Ventana*, con la marca sobre la activa.

| Menú | Acción | Permiso que lo habilita |
|---|---|---|
| Sesión ▸ Inicio | `AbrirHijo<FrmInicio>()` | `VER_INICIO` |
| Sesión ▸ Mi perfil | `AbrirHijo<FrmProfile>()` | `VER_MI_PERFIL` |
| Sesión ▸ Bitácora | `AbrirHijo<FrmEvent>()` | `VER_BITACORA` |
| Sesión ▸ Gestión de roles | `AbrirHijo<FrmRoleManagement>()` | `GESTIONAR_ROLES` |
| Sesión ▸ Cerrar sesión | `FrmLogout` modal; si acepta → `Close()` del MDI | `CERRAR_SESION` |

`FrmMain.AplicarPermisos()` oculta en el constructor las opciones para las que el usuario
no tiene permiso (`sessionBLL.HasPermission(...)`). Ocultar un menú **no es seguridad**:
cada operación sensible de la BLL vuelve a validar el permiso.

El resto del menú:

| Menú | Acción |
|---|---|
| Sesión ▸ Salir | `Close()` |
| Ventana ▸ Cascada | `LayoutMdi(MdiLayout.Cascade)` |
| Ventana ▸ Mosaico horizontal | `LayoutMdi(MdiLayout.TileHorizontal)` |
| Ventana ▸ Mosaico vertical | `LayoutMdi(MdiLayout.TileVertical)` |
| Ventana ▸ Cerrar todas | recorre `MdiChildren.ToList()` y cierra cada hijo |

En *Cerrar todas* se copia la colección con `.ToList()` a propósito: cerrar un hijo
modifica `MdiChildren` mientras se la está recorriendo.

`FrmMain` muestra el usuario en sesión en `lblUsuario`, tomado de
`sessionBLL.CurrentUser` en el constructor; si no hay sesión dice "Sin sesión".

### Flujo de ventanas

```mermaid
graph TD
    P[Program.Main] -->|ShowDialog| FL[FrmLogin - modal]
    FL -->|Registrarse: ShowDialog| FR[FrmRegister - modal]
    FR -->|OK: precarga usuario| FL
    FL -->|DialogResult.OK| FM[FrmMain - IsMdiContainer]
    FL -->|Cancel / error| X[Fin de la aplicacion]
    FM -->|Load: AbrirHijo si VER_MI_PERFIL| FP[FrmProfile - hijo MDI]
    FM -->|Menu Perfil| FP
    FM -->|Menu Inicio: VER_INICIO| FLA[FrmInicio - hijo MDI]
    FM -->|Menu Bitacora: VER_BITACORA| FE[FrmEvent - hijo MDI]
    FM -->|Menu Gestion de roles: GESTIONAR_ROLES| FRM[FrmRoleManagement - hijo MDI]
    FM -->|Menu Cerrar sesion: ShowDialog| FLO[FrmLogout - modal]
    FLO -->|OK| C[FrmMain.Close: fin de la aplicacion]
```

---

## 9. Correr el proyecto

```bash
dotnet build ingSoftWinForm/ingSoftWinForm.sln
```

Proyecto de inicio: `GUI` (`01 - Presentation Layer/UI/GUI.csproj`). Antes del primer
arranque hay que tener la base `IF_DB` con sus tablas (`./sql/init-db.sh`) y al menos un
usuario hecho con `create-user.sh` — si no, no hay forma de pasar el login. El paso a
paso está en el [Quick start](#quick-start).

---

## 10. Gestión de perfiles: roles y permisos (T04)

**Objetivo:** que cada usuario tenga uno o más **roles** y que cada rol otorgue
**permisos** simples o compuestos, para decidir qué opciones ve y qué operaciones puede
ejecutar. Se aplica el patrón **Composite** y los árboles se muestran en `TreeView` con
funciones **recursivas**. Documentación completa (DC, DER, DS, CU) en
`docs/ERS - Especificación de Requerimientos Software/`:
`DC-permisos-composite.md`, `DER-permisos-composite.md`, `DS-permisos-composite.md`,
`CU-gestion-perfiles.md` y `DC-modelo-I-vs-II-permisos.md`.

### Modelo (Modelo II: el Rol es una entidad aparte)

| Rol en el patrón | Clase (`BE.Entity`) | Qué es |
|---|---|---|
| *Component* | `Permission` (abstracta) | Un permiso (`Id`, `Code`, `Name`, `GetChildren`, `Add`, `Remove`, `Grants`, `Flatten`) |
| *Leaf* | `SimplePermission` | Permiso atómico, ej. `GESTIONAR_ROLES`. `Add/Remove` lanzan `InvalidOperationException` |
| *Composite* | `CompoundPermission` | Agrupa permisos; rechaza duplicados y **ciclos** |
| — | `Role` | Agrega permisos; **no** forma parte del Composite |
| — | `User.Roles` | Un usuario puede tener varios roles |

```mermaid
classDiagram
    Permission <|-- SimplePermission
    Permission <|-- CompoundPermission
    CompoundPermission o--> "0..*" Permission : children
    Role o--> "0..*" Permission : Permissions
    User o--> "0..*" Role : Roles
```

El **catálogo de permisos es fijo**: se siembra por SQL y la app no crea, modifica ni
elimina permisos (ni simples ni compuestos). Lo que se gestiona es el **ABM de roles** y
agregar o quitar permisos a un rol. (El documento `DC-modelo-I-vs-II-permisos.md` explica
cómo migrar al Modelo I, donde el rol es un permiso compuesto más.)

### Roles y permisos sembrados

| Rol | Permisos |
|---|---|
| `invitado` | `SESION_BASICA` (compuesto: `VER_MI_PERFIL` + `CERRAR_SESION`) |
| `client` | `SESION_BASICA` + `VER_INICIO` |
| `moderador` | lo del `client` + `VER_BITACORA` |
| `administrador` | lo del `moderador` + `GESTIONAR_ROLES` + `ASIGNAR_ROLES_USUARIO` |

Los códigos están como constantes en `BE.Entity.PermissionCode`; el rol que recibe todo
registro, en `RoleName.Invitado`.

### Cómo se resuelve un permiso (recursivo)

```csharp
// Permission: este nodo, o algún descendiente, tiene el código. Corta en el primer match.
public bool Grants(string code)
{
    if (Code == code) return true;
    foreach (var child in GetChildren())     // la hoja devuelve lista vacía
        if (child.Grants(code)) return true;
    return false;
}
// Role.Grants  => Permissions.Any(p => p.Grants(code))
// User.HasPermission => Roles.Any(r => r.Grants(code))
// SessionManager.HasPermission(code) => usuario en sesión
```

El árbol se arma en memoria en `PermissionDAL` (dos consultas: `[Permission]` y
`[Permission_Permission]`, y cada compuesto se llena a sí mismo recursivamente) y se
dibuja en `FrmRoleManagement.MostrarRecursivo(TreeNode, Permission)`.

### Pantalla *Gestión de roles* (`UI.Roles.FrmRoleManagement`)

Menú *Sesión ▸ Gestión de roles* (requiere `GESTIONAR_ROLES`). Tres `TreeView`:

| Control | Muestra |
|---|---|
| Estructura jerárquica de roles | Cada rol con su árbol de permisos |
| Permisos efectivos del usuario seleccionado | Roles del usuario elegido en el combo, con sus permisos |
| Catálogo general | `ROLES`, `PERMISOS COMPUESTOS` (con su árbol) y `PERMISOS SIMPLES` |

| Botón | Qué hace | Permiso |
|---|---|---|
| Crear Rol | Crea un rol con el nombre escrito | `GESTIONAR_ROLES` |
| Eliminar Rol | Borra el rol seleccionado | `GESTIONAR_ROLES` |
| Asignar Permiso a Rol | Agrega el permiso del catálogo al rol seleccionado | `GESTIONAR_ROLES` |
| Quitar Permiso de Rol | Quita un permiso asignado directamente al rol | `GESTIONAR_ROLES` |
| Asignar Rol a Usuario | Asigna el rol del catálogo al usuario del combo | `ASIGNAR_ROLES_USUARIO` |
| Quitar Rol a Usuario | Quita el rol seleccionado en *permisos efectivos* | `ASIGNAR_ROLES_USUARIO` |

Reglas (validadas en `RoleBLL`, que devuelve un `OperationResult` en vez de lanzar
excepciones por casos de negocio):

- No se elimina el rol `invitado` ni un rol con usuarios asignados.
- No se agrega un permiso que el rol ya otorga (directo o dentro de un compuesto), ni se
  quita uno que no esté asignado directamente.
- No se duplica un nombre de rol; el nombre es obligatorio y de hasta 50 caracteres.
- Nadie puede quitarse a sí mismo un rol que otorga `GESTIONAR_ROLES`.
- Cada operación valida el permiso de quien la ejecuta; sin permiso queda en bitácora como
  `AccesoNoAutorizado`.
- Si se cambian los roles del usuario que tiene la sesión abierta, el cambio se ve en su
  próximo inicio de sesión (los roles se cargan una vez, en el login).

### Tests (`06 - Tests/Tests`)

xUnit, con DAL en memoria (no tocan la base): Composite (hoja, duplicados, ciclos,
`Grants`, `Flatten`), `HasPermission` (directo, anidado, varios roles, sin sesión),
`RoleBLL` (autorización, ABM, reglas) y `UserBLL` (registro con rol `invitado`,
validaciones, login que carga los roles). Los que abren sesión comparten la colección
`"Session"` porque `SessionManager` es un Singleton con estado estático.

```bash
dotnet test "ingSoftWinForm/06 - Tests/Tests/Tests.csproj"
```
