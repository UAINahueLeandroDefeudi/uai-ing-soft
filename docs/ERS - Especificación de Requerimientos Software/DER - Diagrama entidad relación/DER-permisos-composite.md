# DER - Roles y permisos (T04, Composite - Modelo I)

Persistencia de roles y del árbol de permisos. Script: `sql/04_init_create_roles_permission.sql`.

Permisos simples, compuestos y roles comparten una única tabla (`Permission`)
porque en el Composite todos son el mismo tipo `Permission`; se distinguen con
`IsFamily` e `IsRol`. La jerarquía (una familia agrupa simples y/u otras familias)
se resuelve con la auto-relación `Permission_Permission` (padre → hijo), que
permite N niveles. El **rol** es una familia con `IsRol = 1`: no hay tabla `Role`.
El usuario tiene N roles mediante `User_Permission`.

| Caso | `IsFamily` | `IsRol` | Rol en el Composite |
|---|---|---|---|
| Permiso simple | `0` | `0` | *Leaf* |
| Permiso compuesto | `1` | `0` | *Composite* |
| Rol | `1` | `1` | *Composite* (asignable a usuarios) |

```mermaid
erDiagram
    USER ||--o{ USER_PERMISSION : "tiene"
    PERMISSION ||--o{ USER_PERMISSION : "se asigna a (roles)"
    PERMISSION ||--o{ PERMISSION_PERMISSION : "es padre de"
    PERMISSION ||--o{ PERMISSION_PERMISSION : "es hijo de"

    USER {
        uniqueidentifier Id PK
        nvarchar Username UK
        nvarchar FirstName
        nvarchar LastName
        bit IsActive
    }

    PERMISSION {
        int Id PK "IDENTITY"
        nvarchar Code UK "Ej: GESTIONAR_ROLES, ROL_ADMINISTRADOR. Es lo que consulta el codigo"
        nvarchar Name "Texto visible en la UI (invitado, client, ...)"
        bit IsFamily "0 = simple (Leaf) | 1 = compuesto (Composite)"
        bit IsRol "1 = familia asignable a usuarios (requiere IsFamily = 1)"
    }

    PERMISSION_PERMISSION {
        int ParentId PK "FK a Permission - debe ser familia (compuesto o rol)"
        int ChildId PK "FK a Permission - simple, compuesto u otro rol"
    }

    USER_PERMISSION {
        uniqueidentifier UserId PK "FK a User"
        int PermissionId PK "FK a Permission (sin cascade)"
    }
```

## Restricciones que no se ven en el diagrama

1. **Un hijo puede tener varios padres** (un permiso se reutiliza en varios
   compuestos) ⇒ `Permission_Permission` es N:M, con PK compuesta (no se repite
   dentro del mismo padre).
2. **`CK_Permission_Permission_NoSelf`**: `ParentId <> ChildId`. Los ciclos de más
   de un nivel los rechaza `CompoundPermission.Add` en memoria.
3. **Sólo las familias tienen hijos**: se garantiza por diseño (el catálogo se
   siembra por script y la aplicación sólo toca roles). `CK_Permission_RolIsFamily`
   impide que una hoja sea rol (`IsRol = 0 OR IsFamily = 1`).
4. **Rol con usuarios no se borra**: `User_Permission → Permission` no tiene `ON
   DELETE CASCADE` (la BLL además lo valida y informa). Al borrar un rol,
   `RoleDAL.Delete` suelta sus filas de `Permission_Permission` y filtra por
   `IsRol = 1`, de modo que una hoja nunca se elimina desde la aplicación.
5. **`Permission.Code` es único e inmutable**: el código fuente referencia los
   códigos (`BE.Entity.PermissionCode`).
6. **Normalización**: 3FN; todas las relaciones N:M tienen tabla intermedia con
   integridad referencial.

## Datos sembrados

| Rol (`Code`) | Permisos |
|---|---|
| `invitado` (`ROL_INVITADO`) | `SESION_BASICA` (compuesto: `VER_MI_PERFIL` + `CERRAR_SESION`) |
| `client` | `SESION_BASICA`, `VER_INICIO` |
| `moderador` | `SESION_BASICA`, `VER_INICIO`, `VER_BITACORA` |
| `administrador` | todo lo del moderador + `GESTIONAR_ROLES`, `ASIGNAR_ROLES_USUARIO` |

El script es idempotente (no dropea): reejecutarlo no pierde roles ni asignaciones
creados desde la aplicación. Si encuentra tablas del Modelo II (`Role`,
`Role_Permission`, `User_Role`, `IsCompound`) las migra a este modelo y las
elimina. Si existe el usuario `admin` (ver `sql/create-user.sh`) queda con el rol
`administrador`.

## Consulta recursiva de armado del árbol

`PermissionDAL` arma el árbol en memoria con dos consultas (`[Permission]` y
`[Permission_Permission]`). Equivalente en SQL con CTE recursivo, para un compuesto dado:

```sql
WITH recursivo AS (
    SELECT pp.ParentId, pp.ChildId
    FROM   [Permission_Permission] pp
    WHERE  pp.ParentId = @ParentId
    UNION ALL
    SELECT pp.ParentId, pp.ChildId
    FROM   [Permission_Permission] pp
    INNER JOIN recursivo r ON r.ChildId = pp.ParentId
)
SELECT r.ParentId, r.ChildId, p.Code, p.Name, p.IsFamily, p.IsRol
FROM   recursivo r
INNER JOIN [Permission] p ON r.ChildId = p.Id;
```

## Diagramas relacionados

- Clases: [DC-permisos-composite.md](../DC%20-%20Diagrama%20de%20clases/DC-permisos-composite.md)
- Secuencia: [DS-permisos-composite.md](../DS%20-%20Diagramas%20de%20secuencia/DS-permisos-composite.md)
- Alternativa (Modelo I): [DC-modelo-I-vs-II-permisos.md](../DC%20-%20Diagrama%20de%20clases/DC-modelo-I-vs-II-permisos.md)
