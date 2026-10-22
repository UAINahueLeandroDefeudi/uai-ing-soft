/* ============================================================
   T06b - Productos: permiso, etiquetas de idioma y DVV inicial
   Base: IF_DB   (ejecutar despues de 06, 07, 08, 04 y 05)
   Idempotente: no duplica permisos ni vinculos, no pisa textos existentes
   y no recalcula DVV ya registrados.

   GESTIONAR_PRODUCTOS -> ABM de productos, ver su historial y restaurar
   una version anterior. Queda asignado al rol administrador.
   ============================================================ */

USE [IF_DB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- Permiso ---------- */

IF OBJECT_ID('[dbo].[Permission]', 'U') IS NOT NULL
BEGIN
    INSERT INTO [dbo].[Permission] ([Code], [Name], [IsFamily], [IsRol])
    SELECT 'GESTIONAR_PRODUCTOS', 'Gestion de productos', 0, 0
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permission] x WHERE x.[Code] = 'GESTIONAR_PRODUCTOS');

    INSERT INTO [dbo].[Permission_Permission] ([ParentId], [ChildId])
    SELECT r.[Id], p.[Id]
    FROM [dbo].[Permission] r
    JOIN [dbo].[Permission] p ON p.[Code] = 'GESTIONAR_PRODUCTOS'
    WHERE r.[Code] = 'ROL_ADMINISTRADOR'
      AND NOT EXISTS (SELECT 1 FROM [dbo].[Permission_Permission] x WHERE x.[ParentId] = r.[Id] AND x.[ChildId] = p.[Id]);
END
GO

/* ---------- DVV inicial de las tablas vacias ----------
   Una tabla verificable sin DVV registrado se informa como alterada. En una base
   que ya tenia digitos cargados (08 no se vuelve a correr) las tablas de producto
   nacen vacias y sin DVV: se registra el DVV de la tabla vacia, SHA256('Tabla|0'). */

INSERT INTO [dbo].[DigitoVerificador] ([Tabla], [DVV], [FechaCalculo])
SELECT t.[Tabla], CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONCAT(t.[Tabla], N'|0')), 2), SYSDATETIME()
FROM (VALUES (N'Producto'), (N'Producto_Historial')) t([Tabla])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[DigitoVerificador] d WHERE d.[Tabla] = t.[Tabla])
  AND (   (t.[Tabla] = N'Producto'           AND NOT EXISTS (SELECT 1 FROM [dbo].[Producto]))
       OR (t.[Tabla] = N'Producto_Historial' AND NOT EXISTS (SELECT 1 FROM [dbo].[Producto_Historial])));
GO

/* ---------- Etiquetas (es / en) ---------- */

DECLARE @T TABLE ([Clave] NVARCHAR(100) NOT NULL, [Es] NVARCHAR(500) NOT NULL, [En] NVARCHAR(500) NULL);
INSERT INTO @T ([Clave], [Es], [En]) VALUES
    (N'FrmMain.mnuProductos',             N'&Productos', N'&Products'),
    (N'FrmProductos.Title',               N'Gestión de productos', N'Product management'),
    (N'FrmProductos.lblProductos',        N'Productos', N'Products'),
    (N'FrmProductos.chkMostrarBajas',     N'Mostrar dados de baja', N'Show inactive'),
    (N'FrmProductos.grpDatos',            N'Datos del producto', N'Product details'),
    (N'FrmProductos.lblCodigo',           N'Código', N'Code'),
    (N'FrmProductos.lblNombre',           N'Nombre', N'Name'),
    (N'FrmProductos.lblDescripcion',      N'Descripción', N'Description'),
    (N'FrmProductos.lblPrecio',           N'Precio', N'Price'),
    (N'FrmProductos.lblStock',            N'Stock', N'Stock'),
    (N'FrmProductos.btnNuevo',            N'Nuevo', N'New'),
    (N'FrmProductos.btnCrear',            N'Crear', N'Create'),
    (N'FrmProductos.btnGuardar',          N'Guardar cambios', N'Save changes'),
    (N'FrmProductos.btnBaja',             N'Dar de baja', N'Deactivate'),
    (N'FrmProductos.lblHistorial',        N'Historial de cambios: cada fila es la copia completa del producto en esa versión', N'Change history: each row is the full copy of the product at that version'),
    (N'FrmProductos.btnRestaurar',        N'Restaurar esta versión', N'Restore this version'),
    (N'FrmProductos.tituloMensaje',       N'Gestión de productos', N'Product management'),
    (N'FrmProductos.estadoActivo',        N'Activo', N'Active'),
    (N'FrmProductos.estadoBaja',          N'Dado de baja', N'Inactive'),
    (N'FrmProductos.estado',              N'{0} · versión {1} · modificado el {2}', N'{0} · version {1} · modified on {2}'),
    (N'FrmProductos.estadoNuevo',         N'Producto nuevo (todavía no guardado)', N'New product (not saved yet)'),
    (N'FrmProductos.confirmarBaja',       N'¿Dar de baja el producto ''{0}''?', N'Deactivate product ''{0}''?'),
    (N'FrmProductos.confirmarRestaurar',  N'¿Restaurar el producto ''{0}'' a la versión {1}? Se guardará como una versión nueva.', N'Restore product ''{0}'' to version {1}? It will be saved as a new version.'),
    (N'FrmProductos.op.Alta',             N'Alta', N'Created'),
    (N'FrmProductos.op.Modificacion',     N'Modificación', N'Modified'),
    (N'FrmProductos.op.Baja',             N'Baja', N'Deactivated'),
    (N'FrmProductos.op.Restauracion',     N'Restauración', N'Restored'),
    (N'FrmProductos.col.Codigo',          N'Código', N'Code'),
    (N'FrmProductos.col.Nombre',          N'Nombre', N'Name'),
    (N'FrmProductos.col.Descripcion',     N'Descripción', N'Description'),
    (N'FrmProductos.col.Precio',          N'Precio', N'Price'),
    (N'FrmProductos.col.Stock',           N'Stock', N'Stock'),
    (N'FrmProductos.col.Estado',          N'Estado', N'Status'),
    (N'FrmProductos.col.Version',         N'Versión', N'Version'),
    (N'FrmProductos.col.FechaModificacion', N'Última modificación', N'Last modified'),
    (N'FrmProductos.col.Operacion',       N'Operación', N'Operation'),
    (N'FrmProductos.col.EventAt',         N'Cuándo', N'When'),
    (N'FrmProductos.col.EventBy',         N'Quién', N'Who'),
    (N'msg.producto.creado',              N'Producto creado.', N'Product created.'),
    (N'msg.producto.actualizado',         N'Producto actualizado.', N'Product updated.'),
    (N'msg.producto.dadoDeBaja',          N'Producto dado de baja.', N'Product deactivated.'),
    (N'msg.producto.restaurado',          N'Se restauró la versión {0} del producto.', N'Version {0} of the product was restored.'),
    (N'msg.producto.codigoInvalido',      N'El código es obligatorio y admite hasta {0} caracteres.', N'The code is required and allows up to {0} characters.'),
    (N'msg.producto.nombreInvalido',      N'El nombre es obligatorio y admite hasta {0} caracteres.', N'The name is required and allows up to {0} characters.'),
    (N'msg.producto.descripcionLarga',    N'La descripción admite hasta {0} caracteres.', N'The description allows up to {0} characters.'),
    (N'msg.producto.precioInvalido',      N'El precio debe ser un número mayor o igual a cero, con hasta dos decimales.', N'The price must be zero or more, with up to two decimals.'),
    (N'msg.producto.stockInvalido',       N'El stock debe ser mayor o igual a cero.', N'Stock must be zero or more.'),
    (N'msg.producto.codigoExiste',        N'Ya existe otro producto con el código ''{0}''.', N'Another product already uses code ''{0}''.'),
    (N'msg.producto.sinCambios',          N'No hay cambios para guardar.', N'There are no changes to save.'),
    (N'msg.producto.yaDadoDeBaja',        N'El producto ya está dado de baja.', N'The product is already inactive.'),
    (N'msg.producto.versionActual',       N'Esa es la versión actual del producto.', N'That is the current version of the product.'),
    (N'msg.producto.desactualizado',      N'Otro usuario modificó el producto mientras lo editaba. Se recargaron los datos: revise y vuelva a intentar.', N'Another user changed the product while you were editing it. The data was reloaded: review it and try again.');

INSERT INTO [dbo].[Etiqueta] ([Clave])
SELECT t.[Clave] FROM @T t
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Etiqueta] e WHERE e.[Clave] = t.[Clave]);

INSERT INTO [dbo].[Traduccion] ([IdEtiqueta], [IdIdioma], [Texto])
SELECT e.[IdEtiqueta], i.[IdIdioma], t.[Es]
FROM @T t
JOIN [dbo].[Etiqueta] e ON e.[Clave] = t.[Clave]
JOIN [dbo].[Idioma]   i ON i.[Codigo] = N'es'
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Traduccion] x WHERE x.[IdEtiqueta] = e.[IdEtiqueta] AND x.[IdIdioma] = i.[IdIdioma]);

INSERT INTO [dbo].[Traduccion] ([IdEtiqueta], [IdIdioma], [Texto])
SELECT e.[IdEtiqueta], i.[IdIdioma], t.[En]
FROM @T t
JOIN [dbo].[Etiqueta] e ON e.[Clave] = t.[Clave]
JOIN [dbo].[Idioma]   i ON i.[Codigo] = N'en'
WHERE t.[En] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [dbo].[Traduccion] x WHERE x.[IdEtiqueta] = e.[IdEtiqueta] AND x.[IdIdioma] = i.[IdIdioma]);
GO

SELECT r.[Name] AS [Rol], p.[Code] AS [Permiso]
FROM [dbo].[Permission_Permission] pp
JOIN [dbo].[Permission] r ON r.[Id] = pp.[ParentId] AND r.[IsRol] = 1
JOIN [dbo].[Permission] p ON p.[Id] = pp.[ChildId]
WHERE p.[Code] = 'GESTIONAR_PRODUCTOS';
GO
