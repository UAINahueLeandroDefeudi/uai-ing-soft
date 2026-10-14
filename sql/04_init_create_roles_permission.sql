/* ============================================================
   T04 Gestion de Perfiles de Usuario - Roles y permisos (Composite, Modelo I)
   Base: IF_DB
   Ejecucion: sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql\04_init_create_roles_permission.sql

   Modelo I: el Rol es un permiso mas del Composite. Tres tablas:
     [Permission]            Component. IsFamily = 0 hoja (permiso simple), 1 familia (compuesto).
                             IsRol = 1 marca las familias que se asignan a usuarios (roles).
     [Permission_Permission] relacion padre-hijo del Composite (familia -> permiso). Un rol
                             contiene permisos simples, familias u otros roles (rol anidado).
     [User_Permission]       roles de cada usuario (N:M, un usuario puede tener varios)

   Los permisos simples (hojas) son un catalogo fijo: se siembran aca y la aplicacion no
   los crea, modifica ni elimina. La aplicacion solo gestiona roles (IsRol = 1) y sus hijos.

   A diferencia de 01, NO se dropean las tablas: son datos gestionables desde la
   aplicacion (roles y asignaciones) y reejecutar no puede borrarlos. El script es
   idempotente y migra una base del Modelo II anterior ([Role], [Role_Permission],
   [User_Role], [Permission].[IsCompound]) sin perder datos.
   Requiere que exista [dbo].[User] (01_create_table_User.sql).
   ============================================================ */
USE [IF_DB];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- Tablas ---------- */

IF OBJECT_ID('[dbo].[Permission]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Permission]
    (
        [Id]       INT IDENTITY(1,1) NOT NULL,
        [Code]     NVARCHAR(100)     NOT NULL,   -- estable, es lo que consulta el sistema (HasPermission)
        [Name]     NVARCHAR(100)     NOT NULL,   -- texto visible en la UI
        [IsFamily] BIT               NOT NULL CONSTRAINT [DF_Permission_IsFamily] DEFAULT 0,
        [IsRol]    BIT               NOT NULL CONSTRAINT [DF_Permission_IsRol] DEFAULT 0,
        CONSTRAINT [PK_Permission]      PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Permission_Code] UNIQUE ([Code]),
        -- Una hoja nunca es rol.
        CONSTRAINT [CK_Permission_RolIsFamily] CHECK ([IsRol] = 0 OR [IsFamily] = 1)
    );
END
GO

IF OBJECT_ID('[dbo].[Permission_Permission]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Permission_Permission]
    (
        [ParentId] INT NOT NULL,   -- familia (compuesto o rol)
        [ChildId]  INT NOT NULL,   -- permiso simple, familia u otro rol
        CONSTRAINT [PK_Permission_Permission]        PRIMARY KEY CLUSTERED ([ParentId], [ChildId]),
        CONSTRAINT [FK_PermPerm_Parent]              FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Permission] ([Id]),
        CONSTRAINT [FK_PermPerm_Child]               FOREIGN KEY ([ChildId])  REFERENCES [dbo].[Permission] ([Id]),
        CONSTRAINT [CK_Permission_Permission_NoSelf] CHECK ([ParentId] <> [ChildId])
    );
END
GO

IF OBJECT_ID('[dbo].[User_Permission]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[User_Permission]
    (
        [UserId]       UNIQUEIDENTIFIER NOT NULL,
        [PermissionId] INT              NOT NULL,
        CONSTRAINT [PK_User_Permission]   PRIMARY KEY CLUSTERED ([UserId], [PermissionId]),
        CONSTRAINT [FK_UserPerm_User]       FOREIGN KEY ([UserId])       REFERENCES [dbo].[User] ([Id]),
        -- Sin cascade: un rol con usuarios asignados no se puede borrar (lo valida RoleBLL).
        CONSTRAINT [FK_UserPerm_Permission] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permission] ([Id])
    );
END
GO

/* ---------- Migracion desde el Modelo II (solo si existen las tablas viejas) ---------- */

-- [Permission].[IsCompound] pasa a [IsFamily]; se agrega [IsRol].
IF COL_LENGTH('[dbo].[Permission]', 'IsCompound') IS NOT NULL AND COL_LENGTH('[dbo].[Permission]', 'IsFamily') IS NULL
    EXEC sp_rename '[dbo].[Permission].[IsCompound]', 'IsFamily', 'COLUMN';
GO
IF COL_LENGTH('[dbo].[Permission]', 'IsRol') IS NULL
    EXEC('ALTER TABLE [dbo].[Permission] ADD [IsRol] BIT NOT NULL CONSTRAINT [DF_Permission_IsRol] DEFAULT 0;');
GO
-- Una hoja nunca es rol (en bases nuevas ya viene en el CREATE TABLE).
IF OBJECT_ID('[dbo].[CK_Permission_RolIsFamily]') IS NULL
    EXEC('ALTER TABLE [dbo].[Permission] ADD CONSTRAINT [CK_Permission_RolIsFamily] CHECK ([IsRol] = 0 OR [IsFamily] = 1);');
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[Permission]') AND name = 'Code' AND max_length < 200)
    EXEC('ALTER TABLE [dbo].[Permission] ALTER COLUMN [Code] NVARCHAR(100) NOT NULL;');
GO

IF OBJECT_ID('[dbo].[Role]', 'U') IS NOT NULL
BEGIN
    -- Cada rol pasa a ser una familia con IsRol = 1 (Code = ROL_<NOMBRE>).
    EXEC('INSERT INTO [dbo].[Permission] ([Code], [Name], [IsFamily], [IsRol])
          SELECT N''ROL_'' + UPPER(REPLACE(r.[Name], '' '', ''_'')), r.[Name], 1, 1
          FROM [dbo].[Role] r
          WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission] x
                            WHERE x.[Code] = N''ROL_'' + UPPER(REPLACE(r.[Name], '' '', ''_'')));');

    IF OBJECT_ID('[dbo].[Role_Permission]', 'U') IS NOT NULL
        EXEC('INSERT INTO [dbo].[Permission_Permission] ([ParentId], [ChildId])
              SELECT rp_parent.[Id], rp.[PermissionId]
              FROM [dbo].[Role_Permission] rp
              JOIN [dbo].[Role] r ON r.[Id] = rp.[RoleId]
              JOIN [dbo].[Permission] rp_parent ON rp_parent.[Code] = N''ROL_'' + UPPER(REPLACE(r.[Name], '' '', ''_''))
              WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission_Permission] x
                                WHERE x.[ParentId] = rp_parent.[Id] AND x.[ChildId] = rp.[PermissionId]);');

    IF OBJECT_ID('[dbo].[User_Role]', 'U') IS NOT NULL
        EXEC('INSERT INTO [dbo].[User_Permission] ([UserId], [PermissionId])
              SELECT ur.[UserId], p.[Id]
              FROM [dbo].[User_Role] ur
              JOIN [dbo].[Role] r ON r.[Id] = ur.[RoleId]
              JOIN [dbo].[Permission] p ON p.[Code] = N''ROL_'' + UPPER(REPLACE(r.[Name], '' '', ''_''))
              WHERE NOT EXISTS (SELECT 1 FROM [dbo].[User_Permission] x
                                WHERE x.[UserId] = ur.[UserId] AND x.[PermissionId] = p.[Id]);');

    IF OBJECT_ID('[dbo].[User_Role]', 'U') IS NOT NULL EXEC('DROP TABLE [dbo].[User_Role];');
    IF OBJECT_ID('[dbo].[Role_Permission]', 'U') IS NOT NULL EXEC('DROP TABLE [dbo].[Role_Permission];');
    EXEC('DROP TABLE [dbo].[Role];');
END
GO

/* ---------- Seed: catalogo de permisos (hojas y familias no rol) ---------- */

DECLARE @Permissions TABLE ([Code] NVARCHAR(100), [Name] NVARCHAR(100), [IsFamily] BIT);
INSERT INTO @Permissions ([Code], [Name], [IsFamily]) VALUES
    ('VER_MI_PERFIL',         'Ver mi perfil',            0),
    ('CERRAR_SESION',         'Cerrar sesion',            0),
    ('VER_INICIO',            'Ver inicio',               0),
    ('VER_BITACORA',          'Ver bitacora',             0),
    ('GESTIONAR_ROLES',       'Gestion de roles',         0),
    ('ASIGNAR_ROLES_USUARIO', 'Asignar roles a usuarios', 0),
    ('SESION_BASICA',         'Sesion basica',            1);

INSERT INTO [dbo].[Permission] ([Code], [Name], [IsFamily], [IsRol])
SELECT p.[Code], p.[Name], p.[IsFamily], 0
FROM @Permissions p
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission] x WHERE x.[Code] = p.[Code]);
GO

/* ---------- Seed: Composite (familia -> hijo) ---------- */

DECLARE @Tree TABLE ([ParentCode] NVARCHAR(100), [ChildCode] NVARCHAR(100));
INSERT INTO @Tree ([ParentCode], [ChildCode]) VALUES
    ('SESION_BASICA', 'VER_MI_PERFIL'),
    ('SESION_BASICA', 'CERRAR_SESION');

INSERT INTO [dbo].[Permission_Permission] ([ParentId], [ChildId])
SELECT parent.[Id], child.[Id]
FROM @Tree t
JOIN [dbo].[Permission] parent ON parent.[Code] = t.[ParentCode]
JOIN [dbo].[Permission] child  ON child.[Code]  = t.[ChildCode]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission_Permission] pp
                  WHERE pp.[ParentId] = parent.[Id] AND pp.[ChildId] = child.[Id]);
GO

/* ---------- Seed: roles (familias con IsRol = 1) y sus hijos ---------- */

DECLARE @Roles TABLE ([Code] NVARCHAR(100), [Name] NVARCHAR(100));
INSERT INTO @Roles ([Code], [Name]) VALUES
    ('ROL_INVITADO',      'invitado'),
    ('ROL_CLIENT',        'client'),
    ('ROL_MODERADOR',     'moderador'),
    ('ROL_ADMINISTRADOR', 'administrador');

INSERT INTO [dbo].[Permission] ([Code], [Name], [IsFamily], [IsRol])
SELECT r.[Code], r.[Name], 1, 1
FROM @Roles r
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission] x WHERE x.[Code] = r.[Code]);

DECLARE @RolePermissions TABLE ([RoleCode] NVARCHAR(100), [PermissionCode] NVARCHAR(100));
INSERT INTO @RolePermissions ([RoleCode], [PermissionCode]) VALUES
    -- invitado: solo ver su perfil y cerrar sesion (permiso compuesto)
    ('ROL_INVITADO',      'SESION_BASICA'),
    -- client: sesion basica + inicio
    ('ROL_CLIENT',        'SESION_BASICA'),
    ('ROL_CLIENT',        'VER_INICIO'),
    -- moderador: lo del client + bitacora
    ('ROL_MODERADOR',     'SESION_BASICA'),
    ('ROL_MODERADOR',     'VER_INICIO'),
    ('ROL_MODERADOR',     'VER_BITACORA'),
    -- administrador: lo del moderador + gestion de roles
    ('ROL_ADMINISTRADOR', 'SESION_BASICA'),
    ('ROL_ADMINISTRADOR', 'VER_INICIO'),
    ('ROL_ADMINISTRADOR', 'VER_BITACORA'),
    ('ROL_ADMINISTRADOR', 'GESTIONAR_ROLES'),
    ('ROL_ADMINISTRADOR', 'ASIGNAR_ROLES_USUARIO');

INSERT INTO [dbo].[Permission_Permission] ([ParentId], [ChildId])
SELECT r.[Id], p.[Id]
FROM @RolePermissions rp
JOIN [dbo].[Permission] r ON r.[Code] = rp.[RoleCode]
JOIN [dbo].[Permission] p ON p.[Code] = rp.[PermissionCode]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission_Permission] x
                  WHERE x.[ParentId] = r.[Id] AND x.[ChildId] = p.[Id]);
GO

/* ---------- Seed: el usuario 'admin' (create-user.sh) queda como administrador ---------- */

INSERT INTO [dbo].[User_Permission] ([UserId], [PermissionId])
SELECT u.[Id], r.[Id]
FROM [dbo].[User] u
CROSS JOIN [dbo].[Permission] r
WHERE u.[Username] = 'admin' AND r.[Code] = 'ROL_ADMINISTRADOR'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[User_Permission] x WHERE x.[UserId] = u.[Id] AND x.[PermissionId] = r.[Id]);
GO

SELECT r.[Name] AS [Rol], p.[Code] AS [Permiso], p.[IsFamily]
FROM [dbo].[Permission_Permission] pp
JOIN [dbo].[Permission] r ON r.[Id] = pp.[ParentId] AND r.[IsRol] = 1
JOIN [dbo].[Permission] p ON p.[Id] = pp.[ChildId]
ORDER BY r.[Id], p.[Id];
GO
