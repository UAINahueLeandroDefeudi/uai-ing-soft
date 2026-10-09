# DS - Gestión de idiomas con patrón Observer (T05)

## Objetivo

Mostrar cómo viaja un cambio de idioma desde el menú hasta cada formulario abierto, qué pasa con una leyenda sin
traducir y cómo el administrador incorpora un idioma nuevo. Ver clases en `DC-idiomas-observer.md`.

## Escenario 1 — Suscripción al abrir y desuscripción al cerrar

```mermaid
sequenceDiagram
    actor U as Usuario
    participant F as FrmTraducible (formulario hijo)
    participant B as IdiomaBLL (Publisher)
    participant D as IdiomaDAL

    U->>F: abre el formulario
    F->>B: suscribe(this)
    Note over B: lanza Exception si ya estaba suscripto
    F->>F: AplicarIdioma()
    loop cada control / ítem de menú con Name
        F->>B: Traducir("<Form>.<Name>")
        B-->>F: Leyenda(Texto, Traducido, Existe)
        alt Existe
            F->>F: control.Text = Texto (+ tooltip si !Traducido)
        end
    end
    F->>F: OnIdiomaAplicado() (textos armados por código)
    U->>F: cierra el formulario
    F->>B: unsuscribe(this)
```

## Escenario 2 — Cambio dinámico de idioma

```mermaid
sequenceDiagram
    actor U as Usuario
    participant M as FrmMain (menú Idioma)
    participant B as IdiomaBLL (Publisher)
    participant D as IdiomaDAL
    participant S as SessionManager
    participant F1 as FrmProfile (Subscriber)
    participant F2 as FrmEvent (Subscriber)

    U->>M: elige "English"
    M->>B: CambiarIdioma("en")
    B->>D: GetByCodigo("en")
    D-->>B: Idioma(activo)
    opt hay sesión
        B->>S: usuario de la sesión
        B->>D: SetUserIdioma(userId, idEn)
        B->>B: bitácora CambiarIdioma
    end
    B->>D: GetTextos(es) y GetTextos(en)
    D-->>B: diccionarios clave -> texto
    B->>B: notifySuscribers()
    par para cada suscriptor
        B->>M: Actualizar(Idioma en)
        M->>M: AplicarIdioma() + refresca menú y barra de estado
    and
        B->>F1: Actualizar(Idioma en)
        F1->>F1: AplicarIdioma() + estado del usuario
    and
        B->>F2: Actualizar(Idioma en)
        F2->>F2: AplicarIdioma() + "(todos)", encabezados, total
    end
    B-->>M: OperationResult.Ok
```

## Escenario 3 — Leyenda sin traducir (fallback al idioma por defecto)

```mermaid
sequenceDiagram
    participant F as FrmTraducible
    participant B as IdiomaBLL

    F->>B: Traducir("FrmRegister.lblEmail")   %% idioma activo: en
    B->>B: ¿está en textosActivo (en)? no
    B->>B: ¿está en textosDefault (es)? sí -> "Email (opcional)"
    B-->>F: Leyenda(Texto="Email (opcional)", Traducido=false, Existe=true)
    F->>F: lblEmail.Text = "Email (opcional)"
    F->>F: ToolTip = "Not translated (showing the default language)"
```

## Escenario 4 — Incorporar un idioma nuevo (administrador)

```mermaid
sequenceDiagram
    actor A as Administrador
    participant I as FrmIdiomas
    participant B as IdiomaBLL
    participant D as IdiomaDAL
    participant M as FrmMain (Subscriber)

    A->>I: código "pt", nombre "Português" -> Crear idioma
    I->>B: CrearIdioma("pt", "Português")
    B->>B: Authorize(GESTIONAR_IDIOMAS)
    alt sin permiso
        B->>B: bitácora AccesoNoAutorizado
        B-->>I: Fail("msg.noPermiso")
    else con permiso
        B->>B: valida código y nombre
        B->>D: GetByCodigo("pt")
        D-->>B: null (no existe)
        B->>D: Insert(Idioma)
        B->>B: bitácora CrearIdioma
        B-->>I: Ok("msg.idioma.creado")
        I->>B: notifySuscribers()
        B->>M: Actualizar(...)
        M->>M: el menú Idioma lista "Português"
    end

    A->>I: elige "pt" y edita la columna Traducción
    I->>B: GuardarTraduccion(pt, item, "Aceitar")
    B->>D: UpsertTraduccion(idEtiqueta, idPt, "Aceitar")
    B->>B: bitácora ActualizarTraduccion
    B-->>I: Ok("msg.idioma.traduccionGuardada")
    I->>I: recarga la grilla (la etiqueta pasa a "Traducido")
```

## Algoritmo de resolución (`IdiomaBLL.Traducir`)

```
Traducir(clave, args):
    si textosActivo contiene clave:
        texto = textosActivo[clave]; traducido = true;  existe = true
    sino si textosDefault contiene clave:
        texto = textosDefault[clave]; traducido = (activo es el default); existe = true
    sino:
        texto = clave; traducido = false; existe = false
    si hay args: texto = Format(texto, args)   (si el formato es inválido, queda el texto sin formatear)
    devolver Leyenda(clave, texto, traducido, existe)
```
