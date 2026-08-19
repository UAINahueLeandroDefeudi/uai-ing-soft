# DS - Permisos RBAC (Composite)

Dos escenarios sobre el mismo árbol de permisos:

1. **Carga** del árbol Familia/Patente desde la base al iniciar sesión.
2. **Verificación** de un permiso (`IsInRole`) mediante recorrido recursivo.

## 1. Carga del árbol de permisos del usuario

```mermaid
sequenceDiagram
    participant SBLL as SesionBLL
    participant PBLL as PermisosBLL
    participant DAL as PermisosDAL
    participant BD as BaseDeDatos
    participant Fam as Familia
    participant Pat as Patente
    participant U as UsuarioBE

    SBLL->>+PBLL: FillUserComponents(Usuario)
    PBLL->>+DAL: FillUserComponents(Usuario)
    DAL->>+BD: SELECT permisos del usuario
    BD-->>-DAL: filas (id, nombre, permiso)
    DAL->>U: Permisos.Clear()

    loop Por cada permiso asignado
        alt permiso IS NULL (es Familia)
            DAL->>+Fam: new Familia(id, nombre)
            Fam-->>-DAL: Familia
            DAL->>+BD: CTE recursivo GetAll(=idFamilia)
            BD-->>-DAL: descendientes (padre, hijo)
            loop Por cada descendiente
                DAL->>DAL: GetComponent(idPadre, lista)
                alt Tiene padre en el árbol
                    DAL->>Fam: AgregarHijo(Componente)
                else Es raíz
                    DAL->>DAL: lista.Add(Componente)
                end
            end
            DAL->>U: Permisos.Add(Familia)
        else permiso NOT NULL (es Patente)
            DAL->>+Pat: new Patente(id, nombre, TipoPermiso)
            Pat-->>-DAL: Patente
            DAL->>U: Permisos.Add(Patente)
        end
    end

    DAL-->>-PBLL: Usuario con árbol armado
    PBLL-->>-SBLL: OK
```

> El `SELECT` decide qué clase instanciar mirando **un solo campo**: si
> `permiso IS NULL` es `Familia` (Composite), si tiene valor es `Patente` (Leaf).

## 2. Verificación de un permiso (recorrido recursivo)

```mermaid
sequenceDiagram
    actor Usuario
    participant UI as FrmUsuarios
    participant Sess as SessionManager
    participant U as UsuarioBE
    participant Adm as Familia Administrador
    participant Seg as Familia Seguridad
    participant Pat as Patente UsuarioAlta

    Usuario->>+UI: AbrirAltaDeUsuario()
    UI->>+Sess: IsInRole(TipoPermiso.UsuarioAlta)
    Sess->>+U: Permisos
    U-->>-Sess: Lista de Componentes

    loop Por cada permiso raíz del usuario
        Sess->>+Adm: TienePermiso(UsuarioAlta)
        Note over Adm: Composite: no evalúa,<br/>delega en sus hijos
        Adm->>+Seg: TienePermiso(UsuarioAlta)
        Seg->>+Pat: TienePermiso(UsuarioAlta)
        Note over Pat: Leaf: compara y responde
        Pat-->>-Seg: true
        Seg-->>-Adm: true
        Adm-->>-Sess: true
    end

    alt Tiene el permiso
        Sess-->>UI: true
        UI-->>Usuario: MostrarFormularioAlta()
    else No lo tiene
        Sess-->>-UI: false
        UI->>UI: RegistrarEnBitacora(AccesoDenegado)
        UI-->>-Usuario: MostrarAccesoDenegado()
    end
```

## Puntos clave

- **El cliente no pregunta el tipo**: `UI` y `SessionManager` sólo invocan
  `TienePermiso()`; el árbol se encarga de resolver. Sin Composite habría un
  `if (esFamilia) ... else ...` repetido en cada punto de control.
- **Corta en el primer `true`**: no recorre el árbol completo.
- **Profundidad N**: una familia puede contener otras familias sin límite; el
  algoritmo no cambia.
- **Ciclos**: si `PermisosBLL.GeneraCiclo()` no valida antes de guardar, este
  recorrido nunca termina. Es la única forma real de romper el patrón.

## Diagramas relacionados

- Clases: [DC-permisos-composite.md](../DC%20-%20Diagrama%20de%20clases/DC-permisos-composite.md)
- Datos: [DER-permisos-composite.md](../DER%20-%20Diagrama%20entidad%20relación/DER-permisos-composite.md)
