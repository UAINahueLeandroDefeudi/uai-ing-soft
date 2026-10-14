/* ============================================================
   Historial de usuario - Tabla [UserHistory]
   Base: IF_DB   (ejecutar despues de 01_create_table_User.sql)

   Una fila por evento del usuario: cuando ingreso, cuando salio, cuando
   erro la contrasena, cuantos intentos fallidos acumulaba, y cada campo
   que se modifico (valor anterior / nuevo).

   Ejemplos:
     EventType='Login'          Success=1  FailedAttempts=0
     EventType='LoginFailed'    Success=0  FailedAttempts=2  Detail='contrasena incorrecta'
     EventType='Blocked'        Success=0  FailedAttempts=3
     EventType='Logout'         Success=1
     EventType='FieldChanged'   FieldName='Email'  OldValue='a@x'  NewValue='b@x'

   NO se guardan PasswordHash ni Salt: un cambio de contrasena es
   EventType='PasswordChanged' sin valores.

   Ejecucion:
       sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql\06_create_table_UserHistory.sql
   ============================================================ */

USE [IF_DB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* Como Bitacora: NO se dropea, reejecutar el script no borra el historial.
   Sin FK a [User] a proposito: la traza sobrevive a la baja del usuario y
   hay eventos de usernames inexistentes (UserId NULL). */
IF OBJECT_ID('[dbo].[UserHistory]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserHistory]
    (
        [Id]             BIGINT IDENTITY(1,1) NOT NULL,

        [UserId]         UNIQUEIDENTIFIER NULL,        -- NULL si el username no existe
        [Username]       NVARCHAR(50)     NOT NULL,    -- lo tipeado / foto del nombre

        -- Login, Logout, LoginFailed, Blocked, Unblocked, PasswordChanged, FieldChanged
        [EventType]      NVARCHAR(30)     NOT NULL,
        [Success]        BIT              NOT NULL CONSTRAINT [DF_UserHistory_Success]        DEFAULT 1,

        -- Intentos fallidos acumulados DESPUES del evento.
        [FailedAttempts] INT              NOT NULL CONSTRAINT [DF_UserHistory_FailedAttempts] DEFAULT 0,

        -- Solo para FieldChanged.
        [FieldName]      NVARCHAR(100)    NULL,
        [OldValue]       NVARCHAR(MAX)    NULL,
        [NewValue]       NVARCHAR(MAX)    NULL,

        [Detail]         NVARCHAR(500)    NULL,
        [EventAt]        DATETIME2        NOT NULL CONSTRAINT [DF_UserHistory_EventAt]        DEFAULT SYSDATETIME(),

        CONSTRAINT [PK_UserHistory] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE INDEX [IX_UserHistory_UserId_EventAt]
        ON [dbo].[UserHistory] ([UserId], [EventAt] DESC);

    CREATE INDEX [IX_UserHistory_EventAt]
        ON [dbo].[UserHistory] ([EventAt] DESC);
END
GO
