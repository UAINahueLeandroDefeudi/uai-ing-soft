/* ============================================================
   T07 - Digitos verificadores
   Base: IF_DB   (ejecutar despues de 06_create_table_UserHistory.sql)

   - Columna [DVH] (SHA-256 en hexadecimal, 64 caracteres) en cada tabla verificable:
     [UserHistory] y [User_Permission].
   - Tabla [DigitoVerificador]: una fila por tabla verificable con su DVV.

   Para proteger una tabla nueva: agregarle [DVH] CHAR(64) NULL aca, sumar su
   descriptor en BE.Entity.EntidadesVerificables y un bloque en 08.

   Idempotente: se puede reejecutar sin perder datos.

   Ejecucion:
       sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql\07_create_digitos_verificadores.sql
   ============================================================ */

USE [IF_DB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('[dbo].[UserHistory]', 'DVH') IS NULL
    ALTER TABLE [dbo].[UserHistory] ADD [DVH] CHAR(64) NULL;
GO

IF COL_LENGTH('[dbo].[User_Permission]', 'DVH') IS NULL
    ALTER TABLE [dbo].[User_Permission] ADD [DVH] CHAR(64) NULL;
GO

IF OBJECT_ID('[dbo].[DigitoVerificador]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DigitoVerificador]
    (
        [Tabla]        NVARCHAR(100) NOT NULL,
        [DVV]          CHAR(64)      NOT NULL,
        [FechaCalculo] DATETIME2     NOT NULL CONSTRAINT [DF_DigitoVerificador_Fecha] DEFAULT SYSDATETIME(),
        CONSTRAINT [PK_DigitoVerificador] PRIMARY KEY CLUSTERED ([Tabla])
    );
END
GO
