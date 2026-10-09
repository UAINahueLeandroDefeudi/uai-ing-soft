/* ============================================================
   T05 Gestion de Multiples Idiomas
   Base: IF_DB
   Ejecucion: sqlcmd -S localhost\SQLEXPRESS -E -C -I -f 65001 -d IF_DB -i sql\05_create_idiomas.sql
   (-f 65001: este archivo es UTF-8 y tiene tildes en los textos sembrados)

     [Idioma]     idiomas disponibles; exactamente uno es el default ('es')
     [Etiqueta]   clave estable que usa el sistema (p. ej. 'FrmLogin.btnAceptar')
     [Traduccion] texto de cada etiqueta en cada idioma (N:M Etiqueta-Idioma)
     [User].IdIdioma  idioma elegido por el usuario (NULL = default)

   Cuando a un idioma le falta la traduccion de una etiqueta, el sistema muestra el
   texto del idioma default y lo marca como "sin traducir" (ver IdiomaBLL.Traducir).
   El idioma 'en' se siembra INCOMPLETO a proposito: lo pendiente se completa desde
   la pantalla de administracion de idiomas.

   Idempotente: crea lo que falta y resiembra sin duplicar ni pisar textos ya editados.
   Requiere [dbo].[User] (01) y el catalogo de permisos (04).
   ============================================================ */
USE [IF_DB];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- Tablas ---------- */

IF OBJECT_ID('[dbo].[Idioma]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Idioma]
    (
        [IdIdioma]  INT IDENTITY(1,1) NOT NULL,
        [Codigo]    NVARCHAR(10)      NOT NULL,   -- 'es', 'en', 'pt'...
        [Nombre]    NVARCHAR(50)      NOT NULL,   -- nombre visible, en el propio idioma
        [EsDefault] BIT               NOT NULL CONSTRAINT [DF_Idioma_EsDefault] DEFAULT 0,
        [Activo]    BIT               NOT NULL CONSTRAINT [DF_Idioma_Activo]    DEFAULT 1,
        CONSTRAINT [PK_Idioma]        PRIMARY KEY CLUSTERED ([IdIdioma] ASC),
        CONSTRAINT [UQ_Idioma_Codigo] UNIQUE ([Codigo])
    );
END
GO

-- A lo sumo un idioma default (indice filtrado: exige QUOTED_IDENTIFIER ON, ver sqlcmd -I).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Idioma_Default' AND object_id = OBJECT_ID('[dbo].[Idioma]'))
    CREATE UNIQUE INDEX [UX_Idioma_Default] ON [dbo].[Idioma] ([EsDefault]) WHERE [EsDefault] = 1;
GO

IF OBJECT_ID('[dbo].[Etiqueta]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Etiqueta]
    (
        [IdEtiqueta] INT IDENTITY(1,1) NOT NULL,
        [Clave]      NVARCHAR(100)     NOT NULL,
        CONSTRAINT [PK_Etiqueta]       PRIMARY KEY CLUSTERED ([IdEtiqueta] ASC),
        CONSTRAINT [UQ_Etiqueta_Clave] UNIQUE ([Clave])
    );
END
GO

IF OBJECT_ID('[dbo].[Traduccion]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Traduccion]
    (
        [IdEtiqueta] INT           NOT NULL,
        [IdIdioma]   INT           NOT NULL,
        [Texto]      NVARCHAR(500) NOT NULL,
        CONSTRAINT [PK_Traduccion]          PRIMARY KEY CLUSTERED ([IdEtiqueta], [IdIdioma]),
        CONSTRAINT [FK_Traduccion_Etiqueta] FOREIGN KEY ([IdEtiqueta]) REFERENCES [dbo].[Etiqueta] ([IdEtiqueta]),
        CONSTRAINT [FK_Traduccion_Idioma]   FOREIGN KEY ([IdIdioma])   REFERENCES [dbo].[Idioma] ([IdIdioma])
    );
END
GO

IF COL_LENGTH('[dbo].[User]', 'IdIdioma') IS NULL
    ALTER TABLE [dbo].[User] ADD [IdIdioma] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_User_Idioma')
    ALTER TABLE [dbo].[User] ADD CONSTRAINT [FK_User_Idioma] FOREIGN KEY ([IdIdioma]) REFERENCES [dbo].[Idioma] ([IdIdioma]);
GO

/* ---------- Seed: idiomas ---------- */

INSERT INTO [dbo].[Idioma] ([Codigo], [Nombre], [EsDefault], [Activo])
SELECT v.[Codigo], v.[Nombre], v.[EsDefault], 1
FROM (VALUES (N'es', N'Español', 1), (N'en', N'English', 0)) AS v ([Codigo], [Nombre], [EsDefault])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Idioma] x WHERE x.[Codigo] = v.[Codigo]);
GO

/* ---------- Seed: etiquetas y traducciones ----------
   Convenciones de clave:
     <Formulario>.<control>      texto de un control (el nombre del control en el Designer)
     <Formulario>.Title          titulo del formulario
     <Formulario>.<algo>         textos que arma el codigo del formulario
     msg.*                       mensajes de la BLL (OperationResult.MessageKey) y de la UI
   El texto en ingles va NULL cuando todavia no esta traducido. */

DECLARE @T TABLE ([Clave] NVARCHAR(100) NOT NULL, [Es] NVARCHAR(500) NOT NULL, [En] NVARCHAR(500) NULL);
INSERT INTO @T ([Clave], [Es], [En]) VALUES
    -- FrmMain
    (N'FrmMain.Title',               N'Sistema de Gestión',       N'Management System'),
    (N'FrmMain.mnuSesion',           N'&Sesión',                  N'&Session'),
    (N'FrmMain.mnuInicio',           N'&Inicio',                  N'&Home'),
    (N'FrmMain.mnuPerfil',           N'Mi &perfil',               N'My &profile'),
    (N'FrmMain.mnuEvent',            N'&Bitácora',                N'&Audit log'),
    (N'FrmMain.mnuRoles',            N'Gestión de &roles',        N'&Role management'),
    (N'FrmMain.mnuCerrarSesion',     N'&Cerrar sesión',           N'&Log out'),
    (N'FrmMain.mnuSalir',            N'S&alir',                   N'E&xit'),
    (N'FrmMain.mnuVentana',          N'&Ventana',                 N'&Window'),
    (N'FrmMain.mnuCascada',          N'Cascada',                  N'Cascade'),
    (N'FrmMain.mnuMosaicoHorizontal',N'Mosaico horizontal',       N'Tile horizontally'),
    (N'FrmMain.mnuMosaicoVertical',  N'Mosaico vertical',         N'Tile vertically'),
    (N'FrmMain.mnuCerrarTodas',      N'Cerrar todas',             N'Close all'),
    (N'FrmMain.mnuIdioma',           N'&Idioma',                  N'&Language'),
    (N'FrmMain.mnuIdiomas',          N'Gestión de i&diomas',      N'&Language management'),
    (N'FrmMain.usuarioFmt',          N'Usuario: {0} ({1} {2})',   N'User: {0} ({1} {2})'),
    (N'FrmMain.sinSesion',           N'Sin sesión',               N'No session'),
    -- FrmLogin
    (N'FrmLogin.Title',              N'Iniciar sesión',           N'Log in'),
    (N'FrmLogin.lblUsername',        N'Usuario',                  N'Username'),
    (N'FrmLogin.lblPassword',        N'Contraseña',               N'Password'),
    (N'FrmLogin.lblIdioma',          N'Idioma',                   N'Language'),
    (N'FrmLogin.btnAceptar',         N'Aceptar',                  N'OK'),
    (N'FrmLogin.btnCancelar',        N'Cancelar',                 N'Cancel'),
    (N'FrmLogin.btnRegistrar',       N'Registrarse',              N'Sign up'),
    (N'msg.login.blocked',           N'El usuario está bloqueado. Contacte al administrador.', N'The user is blocked. Contact the administrator.'),
    (N'msg.login.inactive',          N'El usuario está dado de baja.',                         N'The user has been deactivated.'),
    (N'msg.login.sessionOpen',       N'Ya hay una sesión iniciada.',                           N'There is already an active session.'),
    (N'msg.login.invalid',           N'Usuario o contraseña incorrectos.',                     N'Incorrect username or password.'),
    (N'msg.login.noConnection',      N'No se pudo conectar con el servidor. Intente nuevamente.', N'Could not connect to the server. Please try again.'),
    -- FrmRegister
    (N'FrmRegister.Title',           N'Registrarse',              N'Sign up'),
    (N'FrmRegister.lblUsername',     N'Usuario',                  N'Username'),
    (N'FrmRegister.lblPassword',     N'Contraseña',               N'Password'),
    (N'FrmRegister.lblConfirmPassword', N'Repetir contraseña',    N'Repeat password'),
    (N'FrmRegister.lblFirstName',    N'Nombre',                   N'First name'),
    (N'FrmRegister.lblLastName',     N'Apellido',                 N'Last name'),
    (N'FrmRegister.lblEmail',        N'Email (opcional)',         NULL),
    (N'FrmRegister.btnRegistrar',    N'Registrar',                N'Sign up'),
    (N'FrmRegister.btnCancelar',     N'Cancelar',                 N'Cancel'),
    (N'FrmRegister.msgTitle',        N'Registro',                 NULL),
    -- FrmLogout
    (N'FrmLogout.Title',             N'Cerrar sesión',            N'Log out'),
    (N'FrmLogout.lblPregunta',       N'¿Desea cerrar la sesión?', N'Do you want to log out?'),
    (N'FrmLogout.btnAceptar',        N'Aceptar',                  N'OK'),
    (N'FrmLogout.btnCancelar',       N'Cancelar',                 N'Cancel'),
    -- FrmInicio
    (N'FrmInicio.Title',             N'Inicio',                   NULL),
    (N'FrmInicio.bienvenida',        N'Bienvenido, {0} {1}',      NULL),
    (N'FrmInicio.bienvenidaGenerica',N'Bienvenido',               NULL),
    -- FrmProfile
    (N'FrmProfile.Title',            N'Mi perfil',                NULL),
    (N'FrmProfile.lblUsernameCaption',    N'Usuario',             NULL),
    (N'FrmProfile.lblNombreCaption',      N'Nombre y apellido',   NULL),
    (N'FrmProfile.lblEmailCaption',       N'Email',               NULL),
    (N'FrmProfile.lblEstadoCaption',      N'Estado',              NULL),
    (N'FrmProfile.lblUltimoAccesoCaption',N'Último acceso',       NULL),
    (N'FrmProfile.lblAltaCaption',        N'Fecha de alta',       NULL),
    (N'FrmProfile.lblRolesCaption',       N'Roles y permisos',    NULL),
    (N'FrmProfile.btnCerrar',        N'Cerrar',                   NULL),
    (N'FrmProfile.estadoBloqueado',  N'Bloqueado',                NULL),
    (N'FrmProfile.estadoActivo',     N'Activo',                   NULL),
    (N'FrmProfile.estadoBaja',       N'Dado de baja',             NULL),
    (N'FrmProfile.sinSesion',        N'No hay una sesión iniciada.', NULL),
    -- FrmEvent (bitacora)
    (N'FrmEvent.Title',              N'Bitácora',                 NULL),
    (N'FrmEvent.lblDesdeCaption',    N'Desde',                    NULL),
    (N'FrmEvent.lblHastaCaption',    N'Hasta',                    NULL),
    (N'FrmEvent.lblTipoCaption',     N'Tipo',                     NULL),
    (N'FrmEvent.lblEventoCaption',   N'Evento',                   NULL),
    (N'FrmEvent.lblPrioridadCaption',N'Prioridad',                NULL),
    (N'FrmEvent.btnFiltrar',         N'Filtrar',                  NULL),
    (N'FrmEvent.btnLimpiar',         N'Limpiar filtros',          NULL),
    (N'FrmEvent.todos',              N'(todos)',                  NULL),
    (N'FrmEvent.total',              N'{0} registro(s)',          NULL),
    (N'FrmEvent.errorLectura',       N'No se pudo leer la bitácora.', NULL),
    (N'FrmEvent.col.Id',             N'Id',                       NULL),
    (N'FrmEvent.col.Fecha',          N'Fecha',                    NULL),
    (N'FrmEvent.col.Tipo',           N'Tipo',                     NULL),
    (N'FrmEvent.col.Evento',         N'Evento',                   NULL),
    (N'FrmEvent.col.Prioridad',      N'Prioridad',                NULL),
    (N'FrmEvent.col.IdUsuario',      N'Id usuario',               NULL),
    (N'FrmEvent.col.Usuario',        N'Usuario',                  NULL),
    (N'FrmEvent.col.Email',          N'Email',                    NULL),
    (N'FrmEvent.col.Rol',            N'Rol',                      NULL),
    (N'FrmEvent.col.Permisos',       N'Permisos',                 NULL),
    (N'FrmEvent.col.Detalle',        N'Detalle',                  NULL),
    -- FrmRoleManagement
    (N'FrmRoleManagement.Title',     N'Gestión de Roles',         NULL),
    (N'FrmRoleManagement.lblRoles',  N'Estructura Jerárquica de Roles (Árbol de Permisos)', NULL),
    (N'FrmRoleManagement.lblUsuario',N'Seleccionar Usuario',      NULL),
    (N'FrmRoleManagement.btnAsignarRolUsuario', N'Asignar Rol a Usuario', NULL),
    (N'FrmRoleManagement.btnQuitarRolUsuario',  N'Quitar Rol a Usuario',  NULL),
    (N'FrmRoleManagement.lblEfectivos', N'Permisos Efectivos del Usuario Seleccionado', NULL),
    (N'FrmRoleManagement.btnAsignarPermisoRol', N'Asignar Permiso a Rol', NULL),
    (N'FrmRoleManagement.btnQuitarPermisoRol',  N'Quitar Permiso de Rol', NULL),
    (N'FrmRoleManagement.lblCatalogo',N'Catálogo General de Permisos y Roles Disponibles', NULL),
    (N'FrmRoleManagement.lblNombreRol',N'Nombre del nuevo rol',   NULL),
    (N'FrmRoleManagement.btnCrearRol', N'Crear Rol',              NULL),
    (N'FrmRoleManagement.btnEliminarRol',N'Eliminar Rol',         NULL),
    (N'FrmRoleManagement.nodoRoles',      N'ROLES',               NULL),
    (N'FrmRoleManagement.nodoCompuestos', N'PERMISOS COMPUESTOS', NULL),
    (N'FrmRoleManagement.nodoSimples',    N'PERMISOS SIMPLES',    NULL),
    (N'FrmRoleManagement.nota',      N'Si desea gestionar roles o permisos, contáctese con un administrador.', NULL),
    (N'FrmRoleManagement.confirmarEliminar', N'¿Eliminar el rol ''{0}''?', NULL),
    (N'FrmRoleManagement.selRol',    N'Seleccione un rol.',       NULL),
    (N'FrmRoleManagement.selRolDestino', N'Seleccione el rol destino en la estructura de roles.', NULL),
    (N'FrmRoleManagement.selPermisoCatalogo', N'Seleccione un permiso en el catálogo.', NULL),
    (N'FrmRoleManagement.selPermisoAsignado', N'Seleccione, en la estructura de roles, un permiso asignado directamente a un rol.', NULL),
    (N'FrmRoleManagement.selUsuario',N'Seleccione un usuario.',   NULL),
    (N'FrmRoleManagement.selRolCatalogo', N'Seleccione un rol en el catálogo.', NULL),
    (N'FrmRoleManagement.selRolEfectivo', N'Seleccione, en los permisos efectivos, el rol a quitar.', NULL),
    (N'FrmRoleManagement.errorCarga',N'No se pudo cargar la información de roles', NULL),
    (N'FrmRoleManagement.errorInesperado', N'Error inesperado en la gestión de roles', NULL),
    (N'FrmRoleManagement.reintentar',N'{0}. Intente nuevamente.', NULL),
    (N'FrmRoleManagement.tituloNoCompletado', N'No se pudo completar', NULL),
    (N'FrmRoleManagement.tituloEliminar', N'Eliminar rol',        NULL),
    (N'FrmRoleManagement.tituloError',    N'Error',               NULL),
    -- FrmIdiomas (administracion de idiomas)
    (N'FrmIdiomas.Title',            N'Gestión de idiomas',       NULL),
    (N'FrmIdiomas.lblIdiomas',       N'Idiomas',                  NULL),
    (N'FrmIdiomas.lblCodigo',        N'Código (p. ej. pt)',       NULL),
    (N'FrmIdiomas.lblNombre',        N'Nombre',                   NULL),
    (N'FrmIdiomas.btnCrear',         N'Crear idioma',             NULL),
    (N'FrmIdiomas.chkActivo',        N'Activo',                   NULL),
    (N'FrmIdiomas.btnGuardarIdioma', N'Guardar idioma',           NULL),
    (N'FrmIdiomas.lblTraducciones',  N'Traducciones del idioma seleccionado (editar la columna Traducción)', NULL),
    (N'FrmIdiomas.chkSoloPendientes',N'Solo sin traducir',        NULL),
    (N'FrmIdiomas.resumen',          N'{0} etiqueta(s), {1} sin traducir', NULL),
    (N'FrmIdiomas.col.Clave',        N'Etiqueta',                 NULL),
    (N'FrmIdiomas.col.TextoDefault', N'Texto por defecto',        NULL),
    (N'FrmIdiomas.col.Texto',        N'Traducción',               NULL),
    (N'FrmIdiomas.col.Estado',       N'Estado',                   NULL),
    (N'FrmIdiomas.estadoTraducido',  N'Traducido',                NULL),
    (N'FrmIdiomas.estadoPendiente',  N'Pendiente',                NULL),
    (N'FrmIdiomas.tituloMensaje',    N'Gestión de idiomas',       NULL),
    -- Mensajes generales y de la BLL
    (N'msg.noPermiso',               N'No tenés permiso para realizar esta operación.', N'You do not have permission to perform this operation.'),
    (N'msg.generic.error',           N'No se pudo completar la operación. Intente nuevamente.', N'The operation could not be completed. Please try again.'),
    (N'msg.sinTraducir',             N'Sin traducir (se muestra el idioma por defecto)', N'Not translated (showing the default language)'),
    (N'msg.role.nameRequired',       N'El nombre del rol es obligatorio.', NULL),
    (N'msg.role.nameTooLong',        N'El nombre del rol no puede superar {0} caracteres.', NULL),
    (N'msg.role.exists',             N'Ya existe un rol con ese nombre.', NULL),
    (N'msg.role.created',            N'Rol creado.', NULL),
    (N'msg.role.invitadoSystem',     N'El rol ''invitado'' es del sistema y no se puede eliminar.', NULL),
    (N'msg.role.hasUsers',           N'El rol tiene usuarios asignados. Quitaselo a los usuarios antes de eliminarlo.', NULL),
    (N'msg.role.deleted',            N'Rol eliminado.', NULL),
    (N'msg.role.alreadyGrants',      N'El rol ''{0}'' ya otorga ''{1}''.', NULL),
    (N'msg.role.permAdded',          N'Permiso agregado al rol.', NULL),
    (N'msg.role.permNotDirect',      N'''{0}'' no está asignado directamente al rol ''{1}'' (forma parte de otro permiso compuesto).', NULL),
    (N'msg.role.permRemoved',        N'Permiso quitado del rol.', NULL),
    (N'msg.role.userHas',            N'''{0}'' ya tiene el rol ''{1}''.', NULL),
    (N'msg.role.assigned',           N'Rol asignado al usuario.', NULL),
    (N'msg.role.userHasNot',         N'''{0}'' no tiene el rol ''{1}''.', NULL),
    (N'msg.role.selfRemove',         N'No podés quitarte a vos mismo un rol que otorga la gestión de roles.', NULL),
    (N'msg.role.unassigned',         N'Rol quitado al usuario.', NULL),
    (N'msg.user.usernameLength',     N'El usuario debe tener entre {0} y {1} caracteres.', NULL),
    (N'msg.user.usernameSpaces',     N'El usuario no puede contener espacios.', NULL),
    (N'msg.user.passwordLength',     N'La contraseña debe tener al menos {0} caracteres.', NULL),
    (N'msg.user.passwordMismatch',   N'Las contraseñas no coinciden.', NULL),
    (N'msg.user.namesRequired',      N'El nombre y el apellido son obligatorios.', NULL),
    (N'msg.user.emailInvalid',       N'El email no tiene un formato válido.', NULL),
    (N'msg.user.usernameExists',     N'Ya existe un usuario con ese nombre de usuario.', NULL),
    (N'msg.user.emailExists',        N'Ya existe un usuario con ese email.', NULL),
    (N'msg.user.registered',         N'Usuario registrado. Ya podés iniciar sesión.', NULL),
    (N'msg.user.registerFailed',     N'No se pudo registrar el usuario. Intente nuevamente.', NULL),
    (N'msg.idioma.noDisponible',     N'El idioma ''{0}'' no está disponible.', NULL),
    (N'msg.idioma.codigoInvalido',   N'El código debe tener 2 o 3 letras minúsculas (opcionalmente con región, p. ej. pt-br).', NULL),
    (N'msg.idioma.nombreInvalido',   N'El nombre es obligatorio y no puede superar {0} caracteres.', NULL),
    (N'msg.idioma.yaExiste',         N'Ya existe un idioma con ese código.', NULL),
    (N'msg.idioma.creado',           N'Idioma creado. Todas sus etiquetas quedan pendientes de traducir.', NULL),
    (N'msg.idioma.actualizado',      N'Idioma actualizado.', NULL),
    (N'msg.idioma.defaultNoSeDesactiva', N'El idioma por defecto no se puede desactivar.', NULL),
    (N'msg.idioma.textoLargo',       N'La traducción no puede superar {0} caracteres.', NULL),
    (N'msg.idioma.traduccionGuardada', N'Traducción guardada.', NULL);

-- Etiquetas que faltan
INSERT INTO [dbo].[Etiqueta] ([Clave])
SELECT t.[Clave] FROM @T t
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Etiqueta] e WHERE e.[Clave] = t.[Clave]);

-- Textos que faltan. No se pisan los existentes: si alguien los edito desde el sistema, se respetan.
INSERT INTO [dbo].[Traduccion] ([IdEtiqueta], [IdIdioma], [Texto])
SELECT e.[IdEtiqueta], i.[IdIdioma], t.[Es]
FROM @T t
JOIN [dbo].[Etiqueta] e ON e.[Clave] = t.[Clave]
JOIN [dbo].[Idioma]   i ON i.[Codigo] = N'es'
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Traduccion] x WHERE x.[IdEtiqueta] = e.[IdEtiqueta] AND x.[IdIdioma] = i.[IdIdioma]);

INSERT INTO [dbo].[Traduccion] ([IdEtiqueta], [IdIdioma], [Texto])
SELECT e.[IdEtiqueta], i.[IdIdioma], t.[En]
FROM @T t
JOIN [dbo].[Etiqueta] e ON e.[Clave] = t.[Clave]
JOIN [dbo].[Idioma]   i ON i.[Codigo] = N'en'
WHERE t.[En] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[Traduccion] x WHERE x.[IdEtiqueta] = e.[IdEtiqueta] AND x.[IdIdioma] = i.[IdIdioma]);
GO

/* ---------- Seed: permiso GESTIONAR_IDIOMAS para el administrador ---------- */

IF OBJECT_ID('[dbo].[Permission]', 'U') IS NOT NULL
BEGIN
    INSERT INTO [dbo].[Permission] ([Code], [Name], [IsCompound])
    SELECT 'GESTIONAR_IDIOMAS', 'Gestion de idiomas', 0
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission] WHERE [Code] = 'GESTIONAR_IDIOMAS');

    INSERT INTO [dbo].[Role_Permission] ([RoleId], [PermissionId])
    SELECT r.[Id], p.[Id]
    FROM [dbo].[Role] r
    CROSS JOIN [dbo].[Permission] p
    WHERE r.[Name] = 'administrador' AND p.[Code] = 'GESTIONAR_IDIOMAS'
      AND NOT EXISTS (SELECT 1 FROM [dbo].[Role_Permission] x WHERE x.[RoleId] = r.[Id] AND x.[PermissionId] = p.[Id]);
END
GO

SELECT i.[Codigo], i.[Nombre], i.[EsDefault],
       COUNT(t.[IdEtiqueta]) AS [Traducidas],
       (SELECT COUNT(*) FROM [dbo].[Etiqueta]) - COUNT(t.[IdEtiqueta]) AS [Pendientes]
FROM [dbo].[Idioma] i
LEFT JOIN [dbo].[Traduccion] t ON t.[IdIdioma] = i.[IdIdioma]
GROUP BY i.[Codigo], i.[Nombre], i.[EsDefault];
GO
