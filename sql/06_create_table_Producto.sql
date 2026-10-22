/* ============================================================
   T06b - Control de cambios: tablas [Producto] y [Producto_Historial]
   Base: IF_DB   (ejecutar despues de 01_create_table_User.sql)

   [Producto] es la entidad de negocio. Cada alta, modificacion, baja o
   restauracion copia la fila COMPLETA del producto a [Producto_Historial]
   como una version nueva, con quien la hizo (EventBy) y cuando (EventAt).
   No hay columnas de valor viejo / valor nuevo: lo que cambio se ve
   comparando dos versiones, y cualquier version se puede volver a aplicar.

   La baja es logica (Activo = 0): no se borran productos ni versiones.
   Las columnas DVH las agrega 07_create_digitos_verificadores.sql.

   Idempotente: crea lo que falta, nunca dropea [Producto] ni su historial.
   Ademas elimina la tabla vieja [UserHistory] (reemplazada por esta).

   Ejecucion:
       sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql\06_create_table_Producto.sql
   ============================================================ */

USE [IF_DB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- Limpieza de [UserHistory] (ya no se usa) ---------- */

IF OBJECT_ID('[dbo].[UserHistory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserHistory];

IF OBJECT_ID('[dbo].[DigitoVerificador]', 'U') IS NOT NULL
    DELETE FROM [dbo].[DigitoVerificador] WHERE [Tabla] = N'UserHistory';
GO

/* ---------- [Producto] ---------- */

IF OBJECT_ID('[dbo].[Producto]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Producto]
    (
        [Id]                INT IDENTITY(1,1) NOT NULL,
        [Codigo]            NVARCHAR(20)      NOT NULL,
        [Nombre]            NVARCHAR(100)     NOT NULL,
        [Descripcion]       NVARCHAR(500)     NULL,
        [Precio]            DECIMAL(18,2)     NOT NULL,
        [Stock]             INT               NOT NULL,
        [Activo]            BIT               NOT NULL CONSTRAINT [DF_Producto_Activo]  DEFAULT 1,

        -- Ultima version registrada en [Producto_Historial].
        [Version]           INT               NOT NULL CONSTRAINT [DF_Producto_Version] DEFAULT 1,
        [FechaModificacion] DATETIME2         NOT NULL CONSTRAINT [DF_Producto_Fecha]   DEFAULT SYSDATETIME(),

        CONSTRAINT [PK_Producto]        PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Producto_Codigo] UNIQUE ([Codigo]),
        CONSTRAINT [CK_Producto_Precio] CHECK ([Precio] >= 0),
        CONSTRAINT [CK_Producto_Stock]  CHECK ([Stock] >= 0)
    );
END
GO

/* ---------- [Producto_Historial]: copia completa por version ---------- */

IF OBJECT_ID('[dbo].[Producto_Historial]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Producto_Historial]
    (
        [Id]          BIGINT IDENTITY(1,1) NOT NULL,
        [ProductoId]  INT                  NOT NULL,
        [Version]     INT                  NOT NULL,

        -- La fila del producto tal como quedo en esta version.
        [Codigo]      NVARCHAR(20)         NOT NULL,
        [Nombre]      NVARCHAR(100)        NOT NULL,
        [Descripcion] NVARCHAR(500)        NULL,
        [Precio]      DECIMAL(18,2)        NOT NULL,
        [Stock]       INT                  NOT NULL,
        [Activo]      BIT                  NOT NULL,

        -- Que operacion la genero, cuando y quien.
        [Operacion]   NVARCHAR(20)         NOT NULL,
        [EventAt]     DATETIME2            NOT NULL CONSTRAINT [DF_ProductoHistorial_EventAt] DEFAULT SYSDATETIME(),
        [EventBy]     UNIQUEIDENTIFIER     NOT NULL,

        CONSTRAINT [PK_Producto_Historial]          PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_ProductoHistorial_Version]   UNIQUE ([ProductoId], [Version]),
        CONSTRAINT [FK_ProductoHistorial_Producto]  FOREIGN KEY ([ProductoId]) REFERENCES [dbo].[Producto] ([Id]),
        CONSTRAINT [FK_ProductoHistorial_User]      FOREIGN KEY ([EventBy])    REFERENCES [dbo].[User] ([Id]),
        CONSTRAINT [CK_ProductoHistorial_Operacion] CHECK ([Operacion] IN (N'Alta', N'Modificacion', N'Baja', N'Restauracion'))
    );
END
GO
