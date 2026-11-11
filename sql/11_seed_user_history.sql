/* ============================================================
   Control de cambios de usuarios: permiso, etiquetas de idioma y digitos verificadores
   Base: IF_DB   (ejecutar despues de 06, 07, 08, 04 y 05)
   Idempotente: no duplica permisos ni vinculos, no pisa textos existentes
   y no recalcula DVV ya registrados.

   GESTIONAR_USUARIOS -> ABM de usuarios, ver su historial de versiones y restaurar
   una version anterior. Queda asignado al rol administrador.

   Migracion: elimina el permiso GESTIONAR_PRODUCTOS y las etiquetas de la pantalla
   de productos (la entidad Producto se reemplazo por el control de cambios de usuarios).
   ============================================================ */

USE [IF_DB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- Migracion: permiso GESTIONAR_PRODUCTOS ----------
   [User_Permission] es una tabla verificable: si el permiso estaba asignado directamente a algun usuario,
   esas filas se borran y se recalcula su DVV (solo si el DVV registrado coincidia con las filas: una tabla
   ya alterada fuera de la app sigue informandose como alterada). */

IF OBJECT_ID('[dbo].[Permission]', 'U') IS NOT NULL
BEGIN
    DECLARE @CalcDVV TABLE ([Valor] CHAR(64) NULL);
    DECLARE @DvvAntes CHAR(64), @DvvValido BIT = 0, @Quitadas INT = 0;

    IF COL_LENGTH('[dbo].[User_Permission]', 'DVH') IS NOT NULL
       AND OBJECT_ID('[dbo].[DigitoVerificador]', 'U') IS NOT NULL
       AND EXISTS (SELECT 1 FROM [dbo].[DigitoVerificador] WHERE [Tabla] = N'User_Permission')
    BEGIN
        -- DVV de las filas actuales, con la misma formula de 08.
        INSERT INTO @CalcDVV ([Valor])
        SELECT CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(
                   N'User_Permission|', COUNT(*),
                   ISNULL(STRING_AGG(CONVERT(NVARCHAR(MAX), CONCAT(N'|', k.[Clave], N'=', ISNULL(k.[DVH], N''))), N'')
                          WITHIN GROUP (ORDER BY k.[Clave] COLLATE Latin1_General_BIN2), N''))), 2)
        FROM (SELECT CONCAT(CONVERT(NVARCHAR(36), up.[UserId]), N':', up.[PermissionId]) AS [Clave], up.[DVH]
              FROM [dbo].[User_Permission] up) k;

        SELECT @DvvAntes = [Valor] FROM @CalcDVV;
        IF @DvvAntes = (SELECT [DVV] FROM [dbo].[DigitoVerificador] WHERE [Tabla] = N'User_Permission')
            SET @DvvValido = 1;
        DELETE FROM @CalcDVV;
    END

    DELETE up
    FROM [dbo].[User_Permission] up
    JOIN [dbo].[Permission] p ON p.[Id] = up.[PermissionId]
    WHERE p.[Code] = 'GESTIONAR_PRODUCTOS';
    SET @Quitadas = @@ROWCOUNT;

    DELETE pp
    FROM [dbo].[Permission_Permission] pp
    JOIN [dbo].[Permission] p ON p.[Id] = pp.[ParentId] OR p.[Id] = pp.[ChildId]
    WHERE p.[Code] = 'GESTIONAR_PRODUCTOS';

    DELETE FROM [dbo].[Permission] WHERE [Code] = 'GESTIONAR_PRODUCTOS';

    IF @Quitadas > 0 AND @DvvValido = 1
    BEGIN
        UPDATE d
        SET d.[DVV] = (
                SELECT CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(
                           N'User_Permission|', COUNT(*),
                           ISNULL(STRING_AGG(CONVERT(NVARCHAR(MAX), CONCAT(N'|', k.[Clave], N'=', ISNULL(k.[DVH], N''))), N'')
                                  WITHIN GROUP (ORDER BY k.[Clave] COLLATE Latin1_General_BIN2), N''))), 2)
                FROM (SELECT CONCAT(CONVERT(NVARCHAR(36), up.[UserId]), N':', up.[PermissionId]) AS [Clave], up.[DVH]
                      FROM [dbo].[User_Permission] up) k),
            d.[FechaCalculo] = SYSDATETIME()
        FROM [dbo].[DigitoVerificador] d
        WHERE d.[Tabla] = N'User_Permission';
    END
END
GO

/* ---------- Migracion: etiquetas de la pantalla de productos ---------- */

IF OBJECT_ID('[dbo].[Etiqueta]', 'U') IS NOT NULL
BEGIN
    DELETE t
    FROM [dbo].[Traduccion] t
    JOIN [dbo].[Etiqueta] e ON e.[IdEtiqueta] = t.[IdEtiqueta]
    WHERE e.[Clave] LIKE N'FrmProductos.%' OR e.[Clave] = N'FrmMain.mnuProductos' OR e.[Clave] LIKE N'msg.producto.%';

    DELETE FROM [dbo].[Etiqueta]
    WHERE [Clave] LIKE N'FrmProductos.%' OR [Clave] = N'FrmMain.mnuProductos' OR [Clave] LIKE N'msg.producto.%';
END
GO

/* ---------- Permiso ---------- */

IF OBJECT_ID('[dbo].[Permission]', 'U') IS NOT NULL
BEGIN
    INSERT INTO [dbo].[Permission] ([Code], [Name], [IsFamily], [IsRol])
    SELECT 'GESTIONAR_USUARIOS', 'Gestion de usuarios', 0, 0
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission] x WHERE x.[Code] = 'GESTIONAR_USUARIOS');

    INSERT INTO [dbo].[Permission_Permission] ([ParentId], [ChildId])
    SELECT r.[Id], p.[Id]
    FROM [dbo].[Permission] r
    JOIN [dbo].[Permission] p ON p.[Code] = 'GESTIONAR_USUARIOS'
    WHERE r.[Code] = 'ROL_ADMINISTRADOR'
      AND NOT EXISTS (SELECT 1 FROM [dbo].[Permission_Permission] x WHERE x.[ParentId] = r.[Id] AND x.[ChildId] = p.[Id]);
END
GO

/* ---------- DVH y DVV iniciales de [User_History] ----------
   Una tabla verificable sin DVV registrado se informa como alterada. En una base que ya tenia digitos
   cargados (08 no se vuelve a correr) [User_History] nace con las versiones 1 sembradas por 06, sin DVH ni
   DVV: se calcula el DVH de esas filas y se registra el DVV (con la misma formula de 08; tabla vacia: SHA256('Tabla|0')).
   Si ya hay un DVV registrado (08 lo calculo, o la app ya escribio) no se toca nada. */

IF OBJECT_ID('[dbo].[User_History]', 'U') IS NOT NULL
   AND COL_LENGTH('[dbo].[User_History]', 'DVH') IS NOT NULL
   AND OBJECT_ID('[dbo].[DigitoVerificador]', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[DigitoVerificador] WHERE [Tabla] = N'User_History')
BEGIN
    UPDATE h
    SET [DVH] = CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(
            N'User_History',
            CASE WHEN c1.v  IS NULL THEN N'|1:N'  ELSE CONCAT(N'|1:',  DATALENGTH(c1.v)  / 2, N':', c1.v)  END,
            CASE WHEN c2.v  IS NULL THEN N'|2:N'  ELSE CONCAT(N'|2:',  DATALENGTH(c2.v)  / 2, N':', c2.v)  END,
            CASE WHEN c3.v  IS NULL THEN N'|3:N'  ELSE CONCAT(N'|3:',  DATALENGTH(c3.v)  / 2, N':', c3.v)  END,
            CASE WHEN c4.v  IS NULL THEN N'|4:N'  ELSE CONCAT(N'|4:',  DATALENGTH(c4.v)  / 2, N':', c4.v)  END,
            CASE WHEN c5.v  IS NULL THEN N'|5:N'  ELSE CONCAT(N'|5:',  DATALENGTH(c5.v)  / 2, N':', c5.v)  END,
            CASE WHEN c6.v  IS NULL THEN N'|6:N'  ELSE CONCAT(N'|6:',  DATALENGTH(c6.v)  / 2, N':', c6.v)  END,
            CASE WHEN c7.v  IS NULL THEN N'|7:N'  ELSE CONCAT(N'|7:',  DATALENGTH(c7.v)  / 2, N':', c7.v)  END,
            CASE WHEN c8.v  IS NULL THEN N'|8:N'  ELSE CONCAT(N'|8:',  DATALENGTH(c8.v)  / 2, N':', c8.v)  END,
            CASE WHEN c9.v  IS NULL THEN N'|9:N'  ELSE CONCAT(N'|9:',  DATALENGTH(c9.v)  / 2, N':', c9.v)  END,
            CASE WHEN c10.v IS NULL THEN N'|10:N' ELSE CONCAT(N'|10:', DATALENGTH(c10.v) / 2, N':', c10.v) END,
            CASE WHEN c11.v IS NULL THEN N'|11:N' ELSE CONCAT(N'|11:', DATALENGTH(c11.v) / 2, N':', c11.v) END,
            CASE WHEN c12.v IS NULL THEN N'|12:N' ELSE CONCAT(N'|12:', DATALENGTH(c12.v) / 2, N':', c12.v) END,
            CASE WHEN c13.v IS NULL THEN N'|13:N' ELSE CONCAT(N'|13:', DATALENGTH(c13.v) / 2, N':', c13.v) END
        )), 2)
    FROM [dbo].[User_History] h
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[Id])))           c1(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[UserId])))       c2(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[Version])))      c3(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[NickName])))     c4(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[FirstName])))    c5(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[LastName])))     c6(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[Pais])))         c7(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[Provincia])))    c8(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[Direccion])))    c9(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[TipoCambio])))   c10(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[RestauradaDesde]))) c11(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[EventAt], 126))) c12(v)
    CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[EventBy])))      c13(v);

    INSERT INTO [dbo].[DigitoVerificador] ([Tabla], [DVV], [FechaCalculo])
    SELECT N'User_History',
           CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(
               N'User_History|', COUNT(*),
               ISNULL(STRING_AGG(CONVERT(NVARCHAR(MAX), CONCAT(N'|', k.[Clave], N'=', ISNULL(k.[DVH], N''))), N'')
                      WITHIN GROUP (ORDER BY k.[Clave] COLLATE Latin1_General_BIN2), N''))), 2),
           SYSDATETIME()
    FROM (SELECT CONVERT(NVARCHAR(MAX), h.[Id]) AS [Clave], h.[DVH] FROM [dbo].[User_History] h) k;
END
GO

/* ---------- Etiquetas (es / en) ---------- */

DECLARE @T TABLE ([Clave] NVARCHAR(100) NOT NULL, [Es] NVARCHAR(500) NOT NULL, [En] NVARCHAR(500) NULL);
INSERT INTO @T ([Clave], [Es], [En]) VALUES
    (N'FrmMain.mnuUsuarios',                N'&Usuarios', N'&Users'),

    -- FrmUsuarios (ABM)
    (N'FrmUsuarios.Title',                  N'Gestión de usuarios', N'User management'),
    (N'FrmUsuarios.lblUsuarios',            N'Usuarios', N'Users'),
    (N'FrmUsuarios.grpDatos',               N'Datos del usuario', N'User details'),
    (N'FrmUsuarios.lblUsername',            N'Usuario', N'Username'),
    (N'FrmUsuarios.lblNickName',            N'Nick', N'Nickname'),
    (N'FrmUsuarios.lblFirstName',           N'Nombre', N'First name'),
    (N'FrmUsuarios.lblLastName',            N'Apellido', N'Last name'),
    (N'FrmUsuarios.lblPais',                N'País', N'Country'),
    (N'FrmUsuarios.lblProvincia',           N'Provincia', N'State / province'),
    (N'FrmUsuarios.lblDireccion',           N'Dirección', N'Address'),
    (N'FrmUsuarios.btnGuardar',             N'Guardar', N'Save'),
    (N'FrmUsuarios.btnControlCambios',      N'Control de cambios', N'Change control'),
    (N'FrmUsuarios.tituloMensaje',          N'Gestión de usuarios', N'User management'),
    (N'FrmUsuarios.estado',                 N'Versión {0}', N'Version {0}'),
    (N'FrmUsuarios.col.Username',           N'Usuario', N'Username'),
    (N'FrmUsuarios.col.NickName',           N'Nick', N'Nickname'),
    (N'FrmUsuarios.col.FirstName',          N'Nombre', N'First name'),
    (N'FrmUsuarios.col.LastName',           N'Apellido', N'Last name'),
    (N'FrmUsuarios.col.Pais',               N'País', N'Country'),
    (N'FrmUsuarios.col.Provincia',          N'Provincia', N'State / province'),
    (N'FrmUsuarios.col.Direccion',          N'Dirección', N'Address'),
    (N'FrmUsuarios.col.Version',            N'Versión', N'Version'),

    -- FrmControlCambios (historial de versiones)
    (N'FrmControlCambios.Title',            N'Control de Cambios', N'Change Control'),
    (N'FrmControlCambios.lblTitulo',        N'Control de Cambios', N'Change Control'),
    (N'FrmControlCambios.lblPaso1',         N'1. Elegí el usuario', N'1. Choose the user'),
    (N'FrmControlCambios.lblPaso2',         N'2. Historial de versiones: quién, cuándo y qué cambió', N'2. Version history: who, when and what changed'),
    (N'FrmControlCambios.lblPaso3',         N'3. Datos de la versión elegida (en amarillo, lo que cambió respecto de la versión anterior)', N'3. Data of the chosen version (in yellow, what changed from the previous version)'),
    (N'FrmControlCambios.btnRestaurar',     N'Restaurar esta versión', N'Restore this version'),
    (N'FrmControlCambios.btnVolver',        N'Volver', N'Back'),
    (N'FrmControlCambios.tituloMensaje',    N'Control de Cambios', N'Change Control'),
    (N'FrmControlCambios.confirmarRestaurar', N'¿Restaurar la versión {0}? Se guardará como una versión nueva.', N'Restore version {0}? It will be saved as a new version.'),
    (N'FrmControlCambios.col.Version',      N'Versión', N'Version'),
    (N'FrmControlCambios.col.EventAt',      N'Fecha y hora', N'Date and time'),
    (N'FrmControlCambios.col.EventBy',      N'Usuario', N'User'),
    (N'FrmControlCambios.col.TipoCambio',   N'Tipo de cambio', N'Change type'),
    (N'FrmControlCambios.col.Detalle',      N'Detalle', N'Detail'),
    (N'FrmControlCambios.col.Campo',        N'Campo', N'Field'),
    (N'FrmControlCambios.col.ValorAnterior', N'Valor en la versión anterior', N'Value in the previous version'),
    (N'FrmControlCambios.col.ValorActual',  N'Valor en esta versión', N'Value in this version'),
    (N'FrmControlCambios.tipo.Alta',        N'Alta', N'Created'),
    (N'FrmControlCambios.tipo.Modificacion', N'Modificacion', N'Modification'),
    (N'FrmControlCambios.tipo.Restauracion', N'Restauracion', N'Restoration'),
    (N'FrmControlCambios.detalle.alta',     N'Alta', N'Created'),
    (N'FrmControlCambios.detalle.modifico', N'Modificó: {0}', N'Changed: {0}'),
    (N'FrmControlCambios.detalle.restauracion', N'Restauración de la versión {0}', N'Restoration of version {0}'),
    (N'FrmControlCambios.campo.NickName',   N'Nick', N'Nickname'),
    (N'FrmControlCambios.campo.FirstName',  N'Nombre', N'First name'),
    (N'FrmControlCambios.campo.LastName',   N'Apellido', N'Last name'),
    (N'FrmControlCambios.campo.Pais',       N'País', N'Country'),
    (N'FrmControlCambios.campo.Provincia',  N'Provincia', N'State / province'),
    (N'FrmControlCambios.campo.Direccion',  N'Dirección', N'Address'),

    -- Mensajes de la BLL
    (N'msg.usuario.actualizado',            N'Usuario actualizado.', N'User updated.'),
    (N'msg.usuario.restaurado',             N'Se restauró la versión {0} del usuario.', N'Version {0} of the user was restored.'),
    (N'msg.usuario.nickInvalido',           N'El nick admite hasta {0} caracteres.', N'The nickname allows up to {0} characters.'),
    (N'msg.usuario.nombreInvalido',         N'El nombre es obligatorio y admite hasta {0} caracteres.', N'The first name is required and allows up to {0} characters.'),
    (N'msg.usuario.apellidoInvalido',       N'El apellido es obligatorio y admite hasta {0} caracteres.', N'The last name is required and allows up to {0} characters.'),
    (N'msg.usuario.paisInvalido',           N'El país admite hasta {0} caracteres.', N'The country allows up to {0} characters.'),
    (N'msg.usuario.provinciaInvalida',      N'La provincia admite hasta {0} caracteres.', N'The state / province allows up to {0} characters.'),
    (N'msg.usuario.direccionInvalida',      N'La dirección admite hasta {0} caracteres.', N'The address allows up to {0} characters.'),
    (N'msg.usuario.sinCambios',             N'No hay cambios para guardar.', N'There are no changes to save.'),
    (N'msg.usuario.versionActual',          N'Esa es la versión actual del usuario.', N'That is the current version of the user.'),
    (N'msg.usuario.desactualizado',         N'Otro usuario modificó este usuario mientras lo editaba. Se recargaron los datos: revise y vuelva a intentar.', N'Another user changed this user while you were editing it. The data was reloaded: review it and try again.');

INSERT INTO [dbo].[Etiqueta] ([Clave])
SELECT t.[Clave] FROM @T t
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Etiqueta] e WHERE e.[Clave] = t.[Clave]);

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

SELECT r.[Name] AS [Rol], p.[Code] AS [Permiso]
FROM [dbo].[Permission_Permission] pp
JOIN [dbo].[Permission] r ON r.[Id] = pp.[ParentId] AND r.[IsRol] = 1
JOIN [dbo].[Permission] p ON p.[Id] = pp.[ChildId]
WHERE p.[Code] = 'GESTIONAR_USUARIOS';
GO
