/* ============================================================
   T07 - Carga inicial de digitos verificadores (UNA SOLA VEZ)
   Base: IF_DB   (ejecutar despues de 07_create_digitos_verificadores.sql)

   Calcula el DVH de cada fila de [UserHistory] y [User_Permission] y el DVV de
   cada tabla sobre los datos que existen AHORA, que se dan por validos.
   La app no tiene ninguna accion para generar digitos verificadores.

   El formato canonico es el mismo de Services.DigitoVerificador (UTF-16LE):
     DVH = SHA256( Tabla | i:longitud:valor | i:N (null) ... )     i = posicion del atributo (desde 1)
     DVV = SHA256( Tabla | N | clave=DVH | clave=DVH ... )          ordenado por clave (binario)
   con Guid en mayusculas, bit 1/0 y datetime2 estilo 126 (7 decimales).

   Volver a correrlo da por validos los datos actuales: usarlo solo sobre una base
   confiable (p. ej. despues de sql/create-user.sh, que escribe sin digitos).

   Para una tabla nueva: copiar un bloque, ajustando tabla, atributos (en el orden
   de BE.Entity.EntidadesVerificables) y clave.

   Ejecucion:
       sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql\08_seed_digitos_verificadores.sql
   ============================================================ */

USE [IF_DB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- [UserHistory]: Id, UserId, Username, EventType, Success, FailedAttempts,
                              FieldName, OldValue, NewValue, Detail, EventAt ---------- */

UPDATE h
SET [DVH] = CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(
        N'UserHistory',
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
        CASE WHEN c11.v IS NULL THEN N'|11:N' ELSE CONCAT(N'|11:', DATALENGTH(c11.v) / 2, N':', c11.v) END
    )), 2)
FROM [dbo].[UserHistory] h
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[Id])))             c1(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[UserId])))         c2(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[Username])))       c3(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[EventType])))      c4(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[Success])))        c5(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[FailedAttempts]))) c6(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[FieldName])))      c7(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[OldValue])))       c8(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[NewValue])))       c9(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[Detail])))         c10(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), h.[EventAt], 126)))   c11(v);
GO

/* ---------- [User_Permission]: UserId, PermissionId ---------- */

UPDATE up
SET [DVH] = CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(
        N'User_Permission',
        CASE WHEN c1.v IS NULL THEN N'|1:N' ELSE CONCAT(N'|1:', DATALENGTH(c1.v) / 2, N':', c1.v) END,
        CASE WHEN c2.v IS NULL THEN N'|2:N' ELSE CONCAT(N'|2:', DATALENGTH(c2.v) / 2, N':', c2.v) END
    )), 2)
FROM [dbo].[User_Permission] up
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), up.[UserId])))       c1(v)
CROSS APPLY (VALUES (CONVERT(NVARCHAR(MAX), up.[PermissionId]))) c2(v);
GO

/* ---------- DVV por tabla (clave = valores de la PK unidos por ':') ---------- */

DELETE FROM [dbo].[DigitoVerificador] WHERE [Tabla] IN (N'UserHistory', N'User_Permission');

INSERT INTO [dbo].[DigitoVerificador] ([Tabla], [DVV], [FechaCalculo])
SELECT N'UserHistory',
       CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(
           N'UserHistory|', COUNT(*),
           ISNULL(STRING_AGG(CONVERT(NVARCHAR(MAX), CONCAT(N'|', k.[Clave], N'=', ISNULL(k.[DVH], N''))), N'')
                  WITHIN GROUP (ORDER BY k.[Clave] COLLATE Latin1_General_BIN2), N''))), 2),
       SYSDATETIME()
FROM (SELECT CONVERT(NVARCHAR(MAX), h.[Id]) AS [Clave], h.[DVH] FROM [dbo].[UserHistory] h) k;

INSERT INTO [dbo].[DigitoVerificador] ([Tabla], [DVV], [FechaCalculo])
SELECT N'User_Permission',
       CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(
           N'User_Permission|', COUNT(*),
           ISNULL(STRING_AGG(CONVERT(NVARCHAR(MAX), CONCAT(N'|', k.[Clave], N'=', ISNULL(k.[DVH], N''))), N'')
                  WITHIN GROUP (ORDER BY k.[Clave] COLLATE Latin1_General_BIN2), N''))), 2),
       SYSDATETIME()
FROM (SELECT CONCAT(CONVERT(NVARCHAR(36), up.[UserId]), N':', up.[PermissionId]) AS [Clave], up.[DVH]
      FROM [dbo].[User_Permission] up) k;
GO

SELECT [Tabla], [DVV], [FechaCalculo] FROM [dbo].[DigitoVerificador];
GO
