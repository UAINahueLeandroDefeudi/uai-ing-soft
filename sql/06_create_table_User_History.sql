/* ============================================================
   Control de cambios de usuarios: columnas nuevas de [User] y tabla [User_History]
   Base: IF_DB   (ejecutar despues de 01_create_table_User.sql y 05_create_idiomas.sql)

   Seis campos de [User] llevan control de cambios: NickName, FirstName, LastName,
   Pais, Provincia y Direccion. El Username de login NO se versiona.
   Cada alta, modificacion o restauracion copia esos seis campos a [User_History]
   como una version nueva, con quien la hizo (EventBy) y cuando (EventAt).
   No hay columnas de valor viejo / valor nuevo: lo que cambio se ve comparando
   dos versiones, y cualquier version se puede volver a aplicar. Restaurar una
   version agrega una version NUEVA (TipoCambio = Restauracion, RestauradaDesde = N).

   [User].Version es el numero de la ultima version (concurrencia optimista).
   Las columnas DVH las agrega 07_create_digitos_verificadores.sql.

   Idempotente: agrega lo que falta, nunca dropea [User] ni [User_History].
   Migracion: elimina las tablas viejas [Producto_Historial], [Producto] y
   [UserHistory] (entidad de ejemplo reemplazada por este control de cambios)
   y sus digitos verificadores.

   Ejecucion:
       sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql\06_create_table_User_History.sql
   ============================================================ */

USE [IF_DB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- Migracion: limpieza de las tablas viejas ---------- */

IF OBJECT_ID('[dbo].[Producto_Historial]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Producto_Historial];

IF OBJECT_ID('[dbo].[Producto]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Producto];

IF OBJECT_ID('[dbo].[UserHistory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserHistory];

IF OBJECT_ID('[dbo].[DigitoVerificador]', 'U') IS NOT NULL
    DELETE FROM [dbo].[DigitoVerificador]
    WHERE [Tabla] IN (N'Producto', N'Producto_Historial', N'UserHistory');
GO

/* ---------- Columnas nuevas de [User] ---------- */

IF COL_LENGTH('[dbo].[User]', 'NickName') IS NULL
    ALTER TABLE [dbo].[User] ADD [NickName] NVARCHAR(50) NULL;
GO

IF COL_LENGTH('[dbo].[User]', 'Pais') IS NULL
    ALTER TABLE [dbo].[User] ADD [Pais] NVARCHAR(100) NULL;
GO

IF COL_LENGTH('[dbo].[User]', 'Provincia') IS NULL
    ALTER TABLE [dbo].[User] ADD [Provincia] NVARCHAR(100) NULL;
GO

IF COL_LENGTH('[dbo].[User]', 'Direccion') IS NULL
    ALTER TABLE [dbo].[User] ADD [Direccion] NVARCHAR(200) NULL;
GO

-- Ultima version registrada en [User_History].
IF COL_LENGTH('[dbo].[User]', 'Version') IS NULL
    ALTER TABLE [dbo].[User] ADD [Version] INT NOT NULL CONSTRAINT [DF_User_Version] DEFAULT 1;
GO

/* ---------- [User_History]: copia de los seis campos por version ---------- */

IF OBJECT_ID('[dbo].[User_History]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[User_History]
    (
        [Id]              BIGINT IDENTITY(1,1) NOT NULL,
        [UserId]          UNIQUEIDENTIFIER     NOT NULL,
        [Version]         INT                  NOT NULL,

        -- Los seis campos versionados, tal como quedaron en esta version.
        [NickName]        NVARCHAR(50)         NULL,
        [FirstName]       NVARCHAR(100)        NOT NULL,
        [LastName]        NVARCHAR(100)        NOT NULL,
        [Pais]            NVARCHAR(100)        NULL,
        [Provincia]       NVARCHAR(100)        NULL,
        [Direccion]       NVARCHAR(200)        NULL,

        -- Que la genero (se guarda por nombre), de que version viene si fue una restauracion,
        -- cuando y quien. El "Detalle" (que campos cambiaron) NO se guarda: se calcula
        -- comparando con la version anterior.
        [TipoCambio]      NVARCHAR(20)         NOT NULL,
        [RestauradaDesde] INT                  NULL,
        [EventAt]         DATETIME2            NOT NULL CONSTRAINT [DF_UserHistory_EventAt] DEFAULT SYSDATETIME(),
        [EventBy]         UNIQUEIDENTIFIER     NOT NULL,

        CONSTRAINT [PK_User_History]          PRIMARY KEY CLUSTERED ([Id] ASC),
        -- Ademas de impedir versiones repetidas, este UNIQUE es el indice (UserId, Version) de las consultas.
        CONSTRAINT [UQ_UserHistory_Version]   UNIQUE ([UserId], [Version]),
        CONSTRAINT [FK_UserHistory_User]      FOREIGN KEY ([UserId])  REFERENCES [dbo].[User] ([Id]),
        CONSTRAINT [FK_UserHistory_EventBy]   FOREIGN KEY ([EventBy]) REFERENCES [dbo].[User] ([Id]),
        CONSTRAINT [CK_UserHistory_Tipo]      CHECK ([TipoCambio] IN (N'Alta', N'Modificacion', N'Restauracion')),
        CONSTRAINT [CK_UserHistory_Restaurada] CHECK (
            ([TipoCambio] = N'Restauracion' AND [RestauradaDesde] IS NOT NULL)
            OR ([TipoCambio] <> N'Restauracion' AND [RestauradaDesde] IS NULL))
    );
END
GO

/* ---------- Version 1 (Alta) de los usuarios que todavia no tienen historial ----------
   EventBy: el usuario que lo creo (CreatedBy, si es el username de un usuario) o, si no hay, el propio usuario.
   EventAt: la fecha de creacion. DVH queda en NULL: lo completan 08 (base nueva) o 11 (base ya protegida). */

INSERT INTO [dbo].[User_History]
    ([UserId], [Version], [NickName], [FirstName], [LastName], [Pais], [Provincia], [Direccion],
     [TipoCambio], [RestauradaDesde], [EventAt], [EventBy])
SELECT u.[Id], 1, u.[NickName], u.[FirstName], u.[LastName], u.[Pais], u.[Provincia], u.[Direccion],
       N'Alta', NULL, u.[CreatedAt], COALESCE(c.[Id], u.[Id])
FROM [dbo].[User] u
LEFT JOIN [dbo].[User] c ON c.[Username] = u.[CreatedBy]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[User_History] h WHERE h.[UserId] = u.[Id]);
GO

SELECT u.[Username], u.[Version], COUNT(h.[Id]) AS [Versiones]
FROM [dbo].[User] u
LEFT JOIN [dbo].[User_History] h ON h.[UserId] = u.[Id]
GROUP BY u.[Username], u.[Version];
GO
