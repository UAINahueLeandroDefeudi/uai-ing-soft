/* ============================================================
   T07 - Permisos de backup: GESTIONAR_BACKUP y REALIZAR_BACKUP
   Base: IF_DB   (ejecutar despues de 04_init_create_roles_permission.sql)
   Idempotente: no duplica permisos ni vinculos.

   REALIZAR_BACKUP  -> generar un backup (solo con la base integra).
   GESTIONAR_BACKUP -> listar/validar/restaurar backups y entrar con la integridad comprometida.
   Ambos quedan asignados al rol administrador.
   ============================================================ */

USE [IF_DB];
GO

IF OBJECT_ID('[dbo].[Permission]', 'U') IS NOT NULL
BEGIN
    DECLARE @Permisos TABLE ([Code] NVARCHAR(100), [Name] NVARCHAR(100));
    INSERT INTO @Permisos ([Code], [Name]) VALUES
        ('GESTIONAR_BACKUP', 'Gestion de backups'),
        ('REALIZAR_BACKUP',  'Realizar backup');

    INSERT INTO [dbo].[Permission] ([Code], [Name], [IsFamily], [IsRol])
    SELECT p.[Code], p.[Name], 0, 0
    FROM @Permisos p
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission] x WHERE x.[Code] = p.[Code]);

    INSERT INTO [dbo].[Permission_Permission] ([ParentId], [ChildId])
    SELECT r.[Id], p.[Id]
    FROM [dbo].[Permission] r
    JOIN [dbo].[Permission] p ON p.[Code] IN ('GESTIONAR_BACKUP', 'REALIZAR_BACKUP')
    WHERE r.[Code] = 'ROL_ADMINISTRADOR'
      AND NOT EXISTS (SELECT 1 FROM [dbo].[Permission_Permission] x WHERE x.[ParentId] = r.[Id] AND x.[ChildId] = p.[Id]);
END
GO

SELECT r.[Name] AS [Rol], p.[Code] AS [Permiso]
FROM [dbo].[Permission_Permission] pp
JOIN [dbo].[Permission] r ON r.[Id] = pp.[ParentId] AND r.[IsRol] = 1
JOIN [dbo].[Permission] p ON p.[Id] = pp.[ChildId]
WHERE p.[Code] IN ('GESTIONAR_BACKUP', 'REALIZAR_BACKUP');
GO
