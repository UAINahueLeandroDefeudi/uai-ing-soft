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
   El idioma 'en' se siembra completo; los idiomas que se agreguen despues se traducen
   desde la pantalla de administracion de idiomas.

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
   El texto en ingles puede ir NULL si una etiqueta nueva todavia no tiene traduccion. */

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
    (N'FrmRegister.lblEmail',        N'Email (opcional)',         N'Email (optional)'),
    (N'FrmRegister.btnRegistrar',    N'Registrar',                N'Sign up'),
    (N'FrmRegister.btnCancelar',     N'Cancelar',                 N'Cancel'),
    (N'FrmRegister.msgTitle',        N'Registro',                 N'Registration'),
    -- FrmLogout
    (N'FrmLogout.Title',             N'Cerrar sesión',            N'Log out'),
    (N'FrmLogout.lblPregunta',       N'¿Desea cerrar la sesión?', N'Do you want to log out?'),
    (N'FrmLogout.btnAceptar',        N'Aceptar',                  N'OK'),
    (N'FrmLogout.btnCancelar',       N'Cancelar',                 N'Cancel'),
    -- FrmInicio
    (N'FrmInicio.Title',             N'Inicio',                   N'Home'),
    (N'FrmInicio.bienvenida',        N'Bienvenido, {0} {1}',      N'Welcome, {0} {1}'),
    (N'FrmInicio.bienvenidaGenerica',N'Bienvenido',               N'Welcome'),
    -- FrmProfile
    (N'FrmProfile.Title',            N'Mi perfil',                N'My profile'),
    (N'FrmProfile.lblUsernameCaption',    N'Usuario',             N'Username'),
    (N'FrmProfile.lblNombreCaption',      N'Nombre y apellido',   N'Full name'),
    (N'FrmProfile.lblEmailCaption',       N'Email',               N'Email'),
    (N'FrmProfile.lblEstadoCaption',      N'Estado',              N'Status'),
    (N'FrmProfile.lblUltimoAccesoCaption',N'Último acceso',       N'Last access'),
    (N'FrmProfile.lblAltaCaption',        N'Fecha de alta',       N'Registration date'),
    (N'FrmProfile.lblRolesCaption',       N'Roles y permisos',    N'Roles and permissions'),
    (N'FrmProfile.btnCerrar',        N'Cerrar',                   N'Close'),
    (N'FrmProfile.estadoBloqueado',  N'Bloqueado',                N'Blocked'),
    (N'FrmProfile.estadoActivo',     N'Activo',                   N'Active'),
    (N'FrmProfile.estadoBaja',       N'Dado de baja',             N'Deactivated'),
    (N'FrmProfile.sinSesion',        N'No hay una sesión iniciada.', N'There is no active session.'),
    -- FrmEvent (bitacora)
    (N'FrmEvent.Title',              N'Bitácora',                 N'Audit log'),
    (N'FrmEvent.lblDesdeCaption',    N'Desde',                    N'From'),
    (N'FrmEvent.lblHastaCaption',    N'Hasta',                    N'To'),
    (N'FrmEvent.lblTipoCaption',     N'Tipo',                     N'Type'),
    (N'FrmEvent.lblEventoCaption',   N'Evento',                   N'Event'),
    (N'FrmEvent.lblPrioridadCaption',N'Prioridad',                N'Priority'),
    (N'FrmEvent.btnFiltrar',         N'Filtrar',                  N'Filter'),
    (N'FrmEvent.btnLimpiar',         N'Limpiar filtros',          N'Clear filters'),
    (N'FrmEvent.todos',              N'(todos)',                  N'(all)'),
    (N'FrmEvent.total',              N'{0} registro(s)',          N'{0} record(s)'),
    (N'FrmEvent.errorLectura',       N'No se pudo leer la bitácora.', N'Could not read the audit log.'),
    (N'FrmEvent.col.Id',             N'Id',                       N'Id'),
    (N'FrmEvent.col.Fecha',          N'Fecha',                    N'Date'),
    (N'FrmEvent.col.Tipo',           N'Tipo',                     N'Type'),
    (N'FrmEvent.col.Evento',         N'Evento',                   N'Event'),
    (N'FrmEvent.col.Prioridad',      N'Prioridad',                N'Priority'),
    (N'FrmEvent.col.IdUsuario',      N'Id usuario',               N'User id'),
    (N'FrmEvent.col.Usuario',        N'Usuario',                  N'User'),
    (N'FrmEvent.col.Email',          N'Email',                    N'Email'),
    (N'FrmEvent.col.Rol',            N'Rol',                      N'Role'),
    (N'FrmEvent.col.Permisos',       N'Permisos',                 N'Permissions'),
    (N'FrmEvent.col.Detalle',        N'Detalle',                  N'Details'),
    -- FrmRoleManagement
    (N'FrmRoleManagement.Title',     N'Gestión de Roles',         N'Role Management'),
    (N'FrmRoleManagement.lblRoles',  N'Estructura Jerárquica de Roles (Árbol de Permisos)', N'Hierarchical Role Structure (Permission Tree)'),
    (N'FrmRoleManagement.lblUsuario',N'Seleccionar Usuario',      N'Select User'),
    (N'FrmRoleManagement.btnAsignarRolUsuario', N'Asignar Rol a Usuario', N'Assign Role to User'),
    (N'FrmRoleManagement.btnQuitarRolUsuario',  N'Quitar Rol a Usuario',  N'Remove Role from User'),
    (N'FrmRoleManagement.lblEfectivos', N'Permisos Efectivos del Usuario Seleccionado', N'Effective Permissions of the Selected User'),
    (N'FrmRoleManagement.btnAsignarPermisoRol', N'Asignar Permiso a Rol', N'Assign Permission to Role'),
    (N'FrmRoleManagement.btnQuitarPermisoRol',  N'Quitar Permiso de Rol', N'Remove Permission from Role'),
    (N'FrmRoleManagement.lblCatalogo',N'Catálogo General de Permisos y Roles Disponibles', N'General Catalog of Available Permissions and Roles'),
    (N'FrmRoleManagement.lblNombreRol',N'Nombre del nuevo rol',   N'New role name'),
    (N'FrmRoleManagement.btnCrearRol', N'Crear Rol',              N'Create Role'),
    (N'FrmRoleManagement.btnEliminarRol',N'Eliminar Rol',         N'Delete Role'),
    (N'FrmRoleManagement.nodoRoles',      N'ROLES',               N'ROLES'),
    (N'FrmRoleManagement.nodoCompuestos', N'PERMISOS COMPUESTOS', N'COMPOSITE PERMISSIONS'),
    (N'FrmRoleManagement.nodoSimples',    N'PERMISOS SIMPLES',    N'SIMPLE PERMISSIONS'),
    (N'FrmRoleManagement.nota',      N'Si desea gestionar roles o permisos, contáctese con un administrador.', N'If you want to manage roles or permissions, contact an administrator.'),
    (N'FrmRoleManagement.confirmarEliminar', N'¿Eliminar el rol ''{0}''?', N'Delete role ''{0}''?'),
    (N'FrmRoleManagement.selRol',    N'Seleccione un rol.',       N'Select a role.'),
    (N'FrmRoleManagement.selRolDestino', N'Seleccione el rol destino en la estructura de roles.', N'Select the target role in the role structure.'),
    (N'FrmRoleManagement.selPermisoCatalogo', N'Seleccione un permiso en el catálogo.', N'Select a permission in the catalog.'),
    (N'FrmRoleManagement.selPermisoAsignado', N'Seleccione, en la estructura de roles, un permiso asignado directamente a un rol.', N'In the role structure, select a permission assigned directly to a role.'),
    (N'FrmRoleManagement.selUsuario',N'Seleccione un usuario.',   N'Select a user.'),
    (N'FrmRoleManagement.selRolCatalogo', N'Seleccione un rol en el catálogo.', N'Select a role in the catalog.'),
    (N'FrmRoleManagement.selRolEfectivo', N'Seleccione, en los permisos efectivos, el rol a quitar.', N'In the effective permissions, select the role to remove.'),
    (N'FrmRoleManagement.errorCarga',N'No se pudo cargar la información de roles', N'Could not load the role information'),
    (N'FrmRoleManagement.errorInesperado', N'Error inesperado en la gestión de roles', N'Unexpected error in role management'),
    (N'FrmRoleManagement.reintentar',N'{0}. Intente nuevamente.', N'{0}. Please try again.'),
    (N'FrmRoleManagement.tituloNoCompletado', N'No se pudo completar', N'Could not complete'),
    (N'FrmRoleManagement.tituloEliminar', N'Eliminar rol',        N'Delete role'),
    (N'FrmRoleManagement.tituloError',    N'Error',               N'Error'),
    -- FrmIdiomas (administracion de idiomas)
    (N'FrmIdiomas.Title',            N'Gestión de idiomas',       N'Language management'),
    (N'FrmIdiomas.lblIdiomas',       N'Idiomas',                  N'Languages'),
    (N'FrmIdiomas.lblCodigo',        N'Código (p. ej. pt)',       N'Code (e.g. pt)'),
    (N'FrmIdiomas.lblNombre',        N'Nombre',                   N'Name'),
    (N'FrmIdiomas.btnCrear',         N'Crear idioma',             N'Create language'),
    (N'FrmIdiomas.chkActivo',        N'Activo',                   N'Active'),
    (N'FrmIdiomas.btnGuardarIdioma', N'Guardar idioma',           N'Save language'),
    (N'FrmIdiomas.lblTraducciones',  N'Traducciones del idioma seleccionado (editar la columna Traducción)', N'Translations of the selected language (edit the Translation column)'),
    (N'FrmIdiomas.chkSoloPendientes',N'Solo sin traducir',        N'Untranslated only'),
    (N'FrmIdiomas.resumen',          N'{0} etiqueta(s), {1} sin traducir', N'{0} label(s), {1} untranslated'),
    (N'FrmIdiomas.col.Clave',        N'Etiqueta',                 N'Label'),
    (N'FrmIdiomas.col.TextoDefault', N'Texto por defecto',        N'Default text'),
    (N'FrmIdiomas.col.Texto',        N'Traducción',               N'Translation'),
    (N'FrmIdiomas.col.Estado',       N'Estado',                   N'Status'),
    (N'FrmIdiomas.estadoTraducido',  N'Traducido',                N'Translated'),
    (N'FrmIdiomas.estadoPendiente',  N'Pendiente',                N'Pending'),
    (N'FrmIdiomas.tituloMensaje',    N'Gestión de idiomas',       N'Language management'),
    -- Mensajes generales y de la BLL
    (N'msg.noPermiso',               N'No tenés permiso para realizar esta operación.', N'You do not have permission to perform this operation.'),
    (N'msg.generic.error',           N'No se pudo completar la operación. Intente nuevamente.', N'The operation could not be completed. Please try again.'),
    (N'msg.sinTraducir',             N'Sin traducir (se muestra el idioma por defecto)', N'Not translated (showing the default language)'),
    (N'msg.role.nameRequired',       N'El nombre del rol es obligatorio.', N'The role name is required.'),
    (N'msg.role.nameTooLong',        N'El nombre del rol no puede superar {0} caracteres.', N'The role name cannot exceed {0} characters.'),
    (N'msg.role.exists',             N'Ya existe un rol con ese nombre.', N'A role with that name already exists.'),
    (N'msg.role.created',            N'Rol creado.', N'Role created.'),
    (N'msg.role.invitadoSystem',     N'El rol ''invitado'' es del sistema y no se puede eliminar.', N'The ''invitado'' role is a system role and cannot be deleted.'),
    (N'msg.role.hasUsers',           N'El rol tiene usuarios asignados. Quitaselo a los usuarios antes de eliminarlo.', N'The role has assigned users. Remove it from those users before deleting it.'),
    (N'msg.role.deleted',            N'Rol eliminado.', N'Role deleted.'),
    (N'msg.role.alreadyGrants',      N'El rol ''{0}'' ya otorga ''{1}''.', N'The role ''{0}'' already grants ''{1}''.'),
    (N'msg.role.permAdded',          N'Permiso agregado al rol.', N'Permission added to the role.'),
    (N'msg.role.permNotDirect',      N'''{0}'' no está asignado directamente al rol ''{1}'' (forma parte de otro permiso compuesto).', N'''{0}'' is not assigned directly to the role ''{1}'' (it is part of another composite permission).'),
    (N'msg.role.permRemoved',        N'Permiso quitado del rol.', N'Permission removed from the role.'),
    (N'msg.role.userHas',            N'''{0}'' ya tiene el rol ''{1}''.', N'''{0}'' already has the role ''{1}''.'),
    (N'msg.role.assigned',           N'Rol asignado al usuario.', N'Role assigned to the user.'),
    (N'msg.role.userHasNot',         N'''{0}'' no tiene el rol ''{1}''.', N'''{0}'' does not have the role ''{1}''.'),
    (N'msg.role.notRole',            N'El permiso seleccionado no es un rol.', N'The selected permission is not a role.'),
    (N'msg.role.cycle',              N'No se puede agregar ''{0}'' al rol ''{1}'': generaría un ciclo.', N'Cannot add ''{0}'' to the role ''{1}'': it would create a cycle.'),
    (N'msg.role.selfRemove',         N'No podés quitarte a vos mismo un rol que otorga la gestión de roles.', N'You cannot remove from yourself a role that grants role management.'),
    (N'msg.role.unassigned',         N'Rol quitado al usuario.', N'Role removed from the user.'),
    (N'msg.user.usernameLength',     N'El usuario debe tener entre {0} y {1} caracteres.', N'The username must be between {0} and {1} characters long.'),
    (N'msg.user.usernameSpaces',     N'El usuario no puede contener espacios.', N'The username cannot contain spaces.'),
    (N'msg.user.passwordLength',     N'La contraseña debe tener al menos {0} caracteres.', N'The password must be at least {0} characters long.'),
    (N'msg.user.passwordMismatch',   N'Las contraseñas no coinciden.', N'The passwords do not match.'),
    (N'msg.user.namesRequired',      N'El nombre y el apellido son obligatorios.', N'First name and last name are required.'),
    (N'msg.user.emailInvalid',       N'El email no tiene un formato válido.', N'The email format is not valid.'),
    (N'msg.user.usernameExists',     N'Ya existe un usuario con ese nombre de usuario.', N'A user with that username already exists.'),
    (N'msg.user.emailExists',        N'Ya existe un usuario con ese email.', N'A user with that email already exists.'),
    (N'msg.user.registered',         N'Usuario registrado. Ya podés iniciar sesión.', N'User registered. You can now log in.'),
    (N'msg.user.registerFailed',     N'No se pudo registrar el usuario. Intente nuevamente.', N'Could not register the user. Please try again.'),
    (N'msg.idioma.noDisponible',     N'El idioma ''{0}'' no está disponible.', N'The language ''{0}'' is not available.'),
    (N'msg.idioma.codigoInvalido',   N'El código debe tener 2 o 3 letras minúsculas (opcionalmente con región, p. ej. pt-br).', N'The code must have 2 or 3 lowercase letters (optionally with a region, e.g. pt-br).'),
    (N'msg.idioma.nombreInvalido',   N'El nombre es obligatorio y no puede superar {0} caracteres.', N'The name is required and cannot exceed {0} characters.'),
    (N'msg.idioma.yaExiste',         N'Ya existe un idioma con ese código.', N'A language with that code already exists.'),
    (N'msg.idioma.creado',           N'Idioma creado. Todas sus etiquetas quedan pendientes de traducir.', N'Language created. All its labels are pending translation.'),
    (N'msg.idioma.actualizado',      N'Idioma actualizado.', N'Language updated.'),
    (N'msg.idioma.defaultNoSeDesactiva', N'El idioma por defecto no se puede desactivar.', N'The default language cannot be deactivated.'),
    (N'msg.idioma.textoLargo',       N'La traducción no puede superar {0} caracteres.', N'The translation cannot exceed {0} characters.'),
    (N'msg.idioma.traduccionGuardada', N'Traducción guardada.', N'Translation saved.');

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
    INSERT INTO [dbo].[Permission] ([Code], [Name], [IsFamily], [IsRol])
    SELECT 'GESTIONAR_IDIOMAS', 'Gestion de idiomas', 0, 0
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission] WHERE [Code] = 'GESTIONAR_IDIOMAS');

    INSERT INTO [dbo].[Permission_Permission] ([ParentId], [ChildId])
    SELECT r.[Id], p.[Id]
    FROM [dbo].[Permission] r
    CROSS JOIN [dbo].[Permission] p
    WHERE r.[Code] = 'ROL_ADMINISTRADOR' AND p.[Code] = 'GESTIONAR_IDIOMAS'
      AND NOT EXISTS (SELECT 1 FROM [dbo].[Permission_Permission] x WHERE x.[ParentId] = r.[Id] AND x.[ChildId] = p.[Id]);
END
GO

SELECT i.[Codigo], i.[Nombre], i.[EsDefault],
       COUNT(t.[IdEtiqueta]) AS [Traducidas],
       (SELECT COUNT(*) FROM [dbo].[Etiqueta]) - COUNT(t.[IdEtiqueta]) AS [Pendientes]
FROM [dbo].[Idioma] i
LEFT JOIN [dbo].[Traduccion] t ON t.[IdIdioma] = i.[IdIdioma]
GROUP BY i.[Codigo], i.[Nombre], i.[EsDefault];
GO
