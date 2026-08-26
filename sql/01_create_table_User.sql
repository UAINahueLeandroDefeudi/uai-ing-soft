/* ============================================================
   CU-01 Iniciar sesion - Tabla de usuarios
   Base: IF_DB
   NOTA: [User] es palabra reservada en T-SQL, siempre entre corchetes.
   ============================================================ */

USE [IF_DB];
GO

-- Requerido por el indice filtrado de mas abajo. sqlcmd los deja en OFF por defecto.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('[dbo].[User]', 'U') IS NOT NULL
    DROP TABLE [dbo].[User];
GO

CREATE TABLE [dbo].[User]
(
    [Id]             UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_User_Id]             DEFAULT NEWID(),

    -- Credenciales. RNF-Seguridad-01: nunca se guarda la contrasena en claro.
    [Username]       NVARCHAR(50)     NOT NULL,
    [PasswordHash]   VARBINARY(32)    NOT NULL,   -- PBKDF2-SHA256, 32 bytes
    [Salt]           VARBINARY(16)    NOT NULL,   -- salt aleatorio por usuario

    -- Datos del usuario
    [FirstName]      NVARCHAR(100)    NOT NULL,
    [LastName]       NVARCHAR(100)    NOT NULL,
    [Email]          NVARCHAR(150)    NULL,

    -- RNF-Seguridad-02: bloqueo automatico a los 3 intentos fallidos.
    [FailedAttempts] INT              NOT NULL CONSTRAINT [DF_User_FailedAttempts] DEFAULT 0,
    [IsBlocked]      BIT              NOT NULL CONSTRAINT [DF_User_IsBlocked]      DEFAULT 0,
    [IsActive]       BIT              NOT NULL CONSTRAINT [DF_User_IsActive]       DEFAULT 1,
    [LastLoginAt]    DATETIME2        NULL,

    -- Auditoria (refleja BE.Base.BaseAuditEntity)
    [CreatedAt]      DATETIME2        NOT NULL CONSTRAINT [DF_User_CreatedAt]      DEFAULT SYSDATETIME(),
    [CreatedBy]      NVARCHAR(50)     NULL,
    [UpdatedAt]      DATETIME2        NULL,
    [UpdatedBy]      NVARCHAR(50)     NULL,

    CONSTRAINT [PK_User]        PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_User_Username] UNIQUE ([Username])
);
GO

-- Email unico solo entre los que tienen email cargado (un UNIQUE comun solo
-- admitiria un unico NULL en toda la tabla).
-- OJO: al ser un indice filtrado, todo INSERT/UPDATE sobre [User] exige
-- QUOTED_IDENTIFIER ON. La app no se ve afectada (SqlClient ya lo pone en ON);
-- desde sqlcmd hay que usar el flag -I.
CREATE UNIQUE INDEX [UX_User_Email]
    ON [dbo].[User] ([Email])
    WHERE [Email] IS NOT NULL;
GO
