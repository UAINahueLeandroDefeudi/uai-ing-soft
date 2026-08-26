/* ============================================================
   Bitacora de auditoria - Tabla de registros
   Base: IF_DB   (ejecutar despues de 01_create_table_User.sql)

   RNF-Seguridad-03 del CU-01: todo intento de acceso queda auditado.
   Refleja BE.Entity.Bitacora. Ver DER-login.md (entidad BITACORA).

   Ejecucion:
       sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql\03_create_table_Bitacora.sql
   ============================================================ */

USE [IF_DB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* A diferencia de 01_create_table_User.sql, aca NO se dropea la tabla:
   reejecutar el script no puede borrar el historial de auditoria. */
IF OBJECT_ID('[dbo].[Bitacora]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Bitacora]
    (
        [id_bitacora]   INT IDENTITY(1,1) NOT NULL,

        -- Los tres enums de BE.Enum se guardan por nombre y no por ordinal:
        -- la bitacora se consulta con un SELECT suelto y 'Critical' se lee, 4 no.
        [Type]          NVARCHAR(20)      NOT NULL,   -- BitacoraType: Event / Error
        [NameEvent]     NVARCHAR(30)      NOT NULL,   -- NameEvent:    Login, Logout, CrearUsuario, ...
        [Priority]      NVARCHAR(20)      NOT NULL,   -- Priority:     Low / Medium / High / Critical / Fatal

        [Detail]        NVARCHAR(500)     NOT NULL,
        [BitacoraDate]  DATETIME2         NOT NULL CONSTRAINT [DF_Bitacora_BitacoraDate]  DEFAULT SYSDATETIME(),

        -- Foto del usuario en el instante del evento. Se copian los datos en vez de
        -- referenciar a [User] para que la traza sobreviva a una baja o un renombre.
        -- Quedan en '' cuando no hay usuario (login con un username inexistente).
        [IdUser]        NVARCHAR(50)      NOT NULL CONSTRAINT [DF_Bitacora_IdUser]        DEFAULT '',
        [Email]         NVARCHAR(150)     NOT NULL CONSTRAINT [DF_Bitacora_Email]         DEFAULT '',
        [FirstName]     NVARCHAR(100)     NOT NULL CONSTRAINT [DF_Bitacora_FirstName]     DEFAULT '',
        [LastName]      NVARCHAR(100)     NOT NULL CONSTRAINT [DF_Bitacora_LastName]      DEFAULT '',

        -- Roles y permisos que tenia el usuario en ese momento, aplanados a texto.
        [RolesPermisos] NVARCHAR(MAX)     NOT NULL CONSTRAINT [DF_Bitacora_RolesPermisos] DEFAULT '',

        CONSTRAINT [PK_Bitacora] PRIMARY KEY CLUSTERED ([id_bitacora] ASC)
    );

    /* Sin FK contra [User] a proposito: la traza tiene que sobrevivir al borrado
       del usuario, y ademas hay filas sin usuario. */

    -- La consulta natural es "lo ultimo primero" y el filtro por rango de fechas.
    CREATE INDEX [IX_Bitacora_BitacoraDate]
        ON [dbo].[Bitacora] ([BitacoraDate] DESC);
END
GO

SELECT [id_bitacora], [Type], [NameEvent], [Priority], [BitacoraDate],
       [FirstName], [LastName], [Detail]
FROM [dbo].[Bitacora]
ORDER BY [id_bitacora] DESC;
GO
