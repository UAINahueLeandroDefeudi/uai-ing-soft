# DC - Diagrama de clases: Permisos RBAC (patrón Composite)

Aplicación del **Composite** a *Familia / Patente*: el cliente (`SessionManager`,
la UI, la BLL) trata igual a un permiso simple y a un rol completo, porque ambos
son un `Componente`.

| Rol en el patrón | Clase | Qué es en el dominio |
|---|---|---|
| *Component* | `Componente` (abstracta) | Permiso genérico |
| *Leaf* | `Patente` | Permiso atómico sobre una acción del sistema |
| *Composite* | `Familia` | Rol / agrupación de patentes y otras familias |
| *Client* | `SessionManager`, `PermisosBLL`, `FrmPatentesFamilias` | Consumen la interfaz `Componente` |

```mermaid
classDiagram
    direction TB

    class Componente {
        <<abstract>>
        +int Id
        +string Nombre
        +TipoPermiso Permiso
        +IList~Componente~ Hijos*
        +AgregarHijo(Componente c)* void
        +QuitarHijo(Componente c)* void
        +VaciarHijos()* void
        +TienePermiso(TipoPermiso p)* bool
        +ToString() string
    }

    class Familia {
        -IList~Componente~ _hijos
        +Familia()
        +IList~Componente~ Hijos
        +AgregarHijo(Componente c) void
        +QuitarHijo(Componente c) void
        +VaciarHijos() void
        +TienePermiso(TipoPermiso p) bool
    }

    class Patente {
        +IList~Componente~ Hijos
        +AgregarHijo(Componente c) void
        +QuitarHijo(Componente c) void
        +VaciarHijos() void
        +TienePermiso(TipoPermiso p) bool
    }

    class TipoPermiso {
        <<enumeration>>
        UsuarioAlta
        UsuarioBaja
        UsuarioModificacion
        UsuarioConsulta
        PermisoAsignar
        BitacoraConsulta
        BackupRestore
    }

    class Usuario {
        +int Id
        +string Username
        +string Nombre
        +List~Componente~ Permisos
        +TienePermiso(TipoPermiso p) bool
    }

    class PermisosBLL {
        -PermisosRepository _permisos
        +GetAllFamilias() IList~Familia~
        +GetAllPatentes() IList~Patente~
        +GetAll(string familia) IList~Componente~
        +GuardarComponente(Componente c, bool esFamilia) Componente
        +GuardarFamilia(Familia f) void
        +FillFamilyComponents(Familia f) void
        +FillUserComponents(Usuario u) void
        +Existe(Componente c, int id) bool
        +GeneraCiclo(Familia padre, Componente hijo) bool
    }

    class PermisosRepository {
        -GetConnectionString() string
        +GetAll(string familia) IList~Componente~
        +GetAllFamilias() IList~Familia~
        +GetAllPatentes() IList~Patente~
        +GuardarComponente(Componente c, bool esFamilia) Componente
        +GuardarFamilia(Familia f) void
        +FillUserComponents(Usuario u) void
        +FillFamilyComponents(Familia f) void
        -GetComponent(int id, IList~Componente~ lista) Componente
    }

    class SessionManager {
        <<Singleton>>
        +IsInRole(TipoPermiso p) bool
    }

    class FrmPatentesFamilias {
        +CargarArbol() void
        +btnAgregarHijo_Click(sender, e) void
        +btnGuardar_Click(sender, e) void
    }

    Componente <|-- Familia
    Componente <|-- Patente
    Familia    o--> "0..*" Componente : Hijos (recursivo)
    Componente --> "0..1" TipoPermiso : solo la Patente lo usa
    Usuario    o--> "0..*" Componente : Permisos asignados
    SessionManager ..> Componente : recorre el arbol
    SessionManager --> "0..1" Usuario
    PermisosBLL --> PermisosRepository
    PermisosBLL ..> Componente
    PermisosRepository ..> Familia : instancia si permiso es NULL
    PermisosRepository ..> Patente : instancia si permiso tiene valor
    FrmPatentesFamilias ..> PermisosBLL
```

## Implementación de `TienePermiso` (la operación uniforme)

```csharp
// Patente (Leaf): hace el trabajo real, no tiene a quién delegar
public override bool TienePermiso(TipoPermiso p) => Permiso.Equals(p);

// Familia (Composite): delega en los hijos y compone el resultado
public override bool TienePermiso(TipoPermiso p)
{
    foreach (var hijo in _hijos)
        if (hijo.TienePermiso(p)) return true;
    return false;
}
```

El cliente escribe siempre lo mismo, sin preguntar de qué tipo es el permiso:

```csharp
if (SessionManager.GetInstance.IsInRole(TipoPermiso.UsuarioAlta)) { ... }
```

## Notas de diseño

- `Patente.Hijos` devuelve una lista vacía y `AgregarHijo()` no hace nada: es la
  variante "interfaz uniforme" del Composite (transparencia por sobre seguridad).
  La alternativa es lanzar `NotSupportedException`.
- `Familia.Hijos` devuelve una **copia** (`_hijos.ToArray()`) para que nadie
  modifique la colección interna sin pasar por `AgregarHijo` / `QuitarHijo`.
- El recorrido es en profundidad y **corta en el primer match**, por eso el
  costo real es mucho menor que recorrer todo el árbol.
- `GeneraCiclo()` se valida **antes** de armar la relación padre-hijo: sin eso,
  un ciclo cuelga el recorrido recursivo con `StackOverflowException`.
- El árbol vive en memoria dentro de `Usuario.Permisos`; la persistencia se
  describe en [DER-permisos-composite.md](../DER%20-%20Diagrama%20entidad%20relación/DER-permisos-composite.md).
