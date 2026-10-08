/* ============================================================
   T04 Gestion de Perfiles de Usuario - Roles y permisos (Composite, Modelo II)
   Base: IF_DB
   Ejecucion: sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql\04_init_create_roles_permission.sql

   Modelo II: el Rol es una entidad aparte que agrega Permisos. Los permisos forman
   el Composite: simple (hoja) o compuesto (agrupa otros permisos).
     [Permission]            Component: simples (IsCompound = 0) y compuestos (IsCompound = 1)
     [Permission_Permission] relacion padre-hijo del Composite (compuesto -> permiso)
     [Role]                  rol (ABM desde la aplicacion)
     [Role_Permission]       permisos que otorga cada rol
     [User_Role]             roles de cada usuario (N:M, un usuario puede tener varios)

   A diferencia de 01, NO se dropean las tablas: son datos gestionables desde la
   aplicacion (roles y asignaciones) y reejecutar no puede borrarlos. Los permisos
   simples y compuestos son un catalogo fijo: se siembran aca y la aplicacion no los
   crea, modifica ni elimina. El script es idempotente.
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
        [Id]         INT IDENTITY(1,1) NOT NULL,
        [Code]       NVARCHAR(50)      NOT NULL,   -- estable, es lo que consulta el sistema (HasPermission)
        [Name]       NVARCHAR(100)     NOT NULL,   -- texto visible en la UI
        [IsCompound] BIT               NOT NULL CONSTRAINT [DF_Permission_IsCompound] DEFAULT 0,
        CONSTRAINT [PK_Permission]      PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Permission_Code] UNIQUE ([Code])
    );
END
GO

IF OBJECT_ID('[dbo].[Permission_Permission]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Permission_Permission]
    (
        [ParentId] INT NOT NULL,   -- permiso compuesto
        [ChildId]  INT NOT NULL,   -- permiso simple u otro compuesto
        CONSTRAINT [PK_Permission_Permission]        PRIMARY KEY CLUSTERED ([ParentId], [ChildId]),
        CONSTRAINT [FK_PermPerm_Parent]              FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Permission] ([Id]),
        CONSTRAINT [FK_PermPerm_Child]               FOREIGN KEY ([ChildId])  REFERENCES [dbo].[Permission] ([Id]),
        CONSTRAINT [CK_Permission_Permission_NoSelf] CHECK ([ParentId] <> [ChildId])
    );
END
GO

IF OBJECT_ID('[dbo].[Role]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Role]
    (
        [Id]        INT IDENTITY(1,1) NOT NULL,
        [Name]      NVARCHAR(50)      NOT NULL,
        -- Auditoria (refleja BE.Base.BaseAuditEntity)
        [CreatedAt] DATETIME2         NOT NULL CONSTRAINT [DF_Role_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy] NVARCHAR(50)      NULL,
        [UpdatedAt] DATETIME2         NULL,
        [UpdatedBy] NVARCHAR(50)      NULL,
        CONSTRAINT [PK_Role]      PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Role_Name] UNIQUE ([Name])
    );
END
GO

IF OBJECT_ID('[dbo].[Role_Permission]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Role_Permission]
    (
        [RoleId]       INT NOT NULL,
        [PermissionId] INT NOT NULL,
        CONSTRAINT [PK_Role_Permission]      PRIMARY KEY CLUSTERED ([RoleId], [PermissionId]),
        -- Al borrar un rol se van con el sus asignaciones de permisos.
        CONSTRAINT [FK_RolePerm_Role]        FOREIGN KEY ([RoleId])       REFERENCES [dbo].[Role] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePerm_Permission]  FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permission] ([Id])
    );
END
GO

IF OBJECT_ID('[dbo].[User_Role]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[User_Role]
    (
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [RoleId] INT              NOT NULL,
        CONSTRAINT [PK_User_Role]   PRIMARY KEY CLUSTERED ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRole_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]),
        -- Sin cascade: un rol con usuarios asignados no se puede borrar (lo valida RoleBLL).
        CONSTRAINT [FK_UserRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Role] ([Id])
    );
END
GO

/* ---------- Seed: catalogo de permisos ---------- */

DECLARE @Permissions TABLE ([Code] NVARCHAR(50), [Name] NVARCHAR(100), [IsCompound] BIT);
INSERT INTO @Permissions ([Code], [Name], [IsCompound]) VALUES
    ('VER_MI_PERFIL',         'Ver mi perfil',            0),
    ('CERRAR_SESION',         'Cerrar sesion',            0),
    ('VER_INICIO',      'Ver inicio',         0),
    ('VER_BITACORA',          'Ver bitacora',             0),
    ('GESTIONAR_ROLES',       'Gestion de roles',         0),
    ('ASIGNAR_ROLES_USUARIO', 'Asignar roles a usuarios', 0),
    ('SESION_BASICA',         'Sesion basica',            1);

INSERT INTO [dbo].[Permission] ([Code], [Name], [IsCompound])
SELECT p.[Code], p.[Name], p.[IsCompound]
FROM @Permissions p
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission] x WHERE x.[Code] = p.[Code]);
GO

/* ---------- Seed: Composite (compuesto -> hijo) ---------- */

DECLARE @Tree TABLE ([ParentCode] NVARCHAR(50), [ChildCode] NVARCHAR(50));
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

/* ---------- Seed: roles y sus permisos ---------- */

DECLARE @Roles TABLE ([Name] NVARCHAR(50));
INSERT INTO @Roles ([Name]) VALUES ('invitado'), ('client'), ('moderador'), ('administrador');

INSERT INTO [dbo].[Role] ([Name], [CreatedBy])
SELECT r.[Name], 'seed'
FROM @Roles r
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Role] x WHERE x.[Name] = r.[Name]);

DECLARE @RolePermissions TABLE ([RoleName] NVARCHAR(50), [PermissionCode] NVARCHAR(50));
INSERT INTO @RolePermissions ([RoleName], [PermissionCode]) VALUES
    -- invitado: solo ver su perfil y cerrar sesion (permiso compuesto)
    ('invitado',      'SESION_BASICA'),
    -- client: sesion basica + inicio
    ('client',        'SESION_BASICA'),
    ('client',        'VER_INICIO'),
    -- moderador: lo del client + bitacora
    ('moderador',     'SESION_BASICA'),
    ('moderador',     'VER_INICIO'),
    ('moderador',     'VER_BITACORA'),
    -- administrador: lo del moderador + gestion de roles
    ('administrador', 'SESION_BASICA'),
    ('administrador', 'VER_INICIO'),
    ('administrador', 'VER_BITACORA'),
    ('administrador', 'GESTIONAR_ROLES'),
    ('administrador', 'ASIGNAR_ROLES_USUARIO');

INSERT INTO [dbo].[Role_Permission] ([RoleId], [PermissionId])
SELECT r.[Id], p.[Id]
FROM @RolePermissions rp
JOIN [dbo].[Role]       r ON r.[Name] = rp.[RoleName]
JOIN [dbo].[Permission] p ON p.[Code] = rp.[PermissionCode]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Role_Permission] x
                  WHERE x.[RoleId] = r.[Id] AND x.[PermissionId] = p.[Id]);
GO

/* ---------- Seed: el usuario 'admin' (create-user.sh) queda como administrador ---------- */

INSERT INTO [dbo].[User_Role] ([UserId], [RoleId])
SELECT u.[Id], r.[Id]
FROM [dbo].[User] u
CROSS JOIN [dbo].[Role] r
WHERE u.[Username] = 'admin' AND r.[Name] = 'administrador'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[User_Role] x WHERE x.[UserId] = u.[Id] AND x.[RoleId] = r.[Id]);
GO

SELECT r.[Name] AS [Rol], p.[Code] AS [Permiso], p.[IsCompound]
FROM [dbo].[Role_Permission] rp
JOIN [dbo].[Role]       r ON r.[Id] = rp.[RoleId]
JOIN [dbo].[Permission] p ON p.[Id] = rp.[PermissionId]
ORDER BY r.[Id], p.[Id];
GO
