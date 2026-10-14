/* ============================================================
   T07 - Etiquetas de idioma de la verificacion de integridad y el backup
   Base: IF_DB   (ejecutar despues de 05_create_idiomas.sql)
   Idempotente: no pisa textos existentes.
   ============================================================ */

USE [IF_DB];
GO

DECLARE @T TABLE ([Clave] NVARCHAR(100) NOT NULL, [Es] NVARCHAR(500) NOT NULL, [En] NVARCHAR(500) NULL);
INSERT INTO @T ([Clave], [Es], [En]) VALUES
    (N'msg.login.systemError',                N'Se produjo un problema del sistema al iniciar sesión. Intente más tarde.', N'A system problem occurred while signing in. Please try again later.'),
    (N'FrmAlertaIntegridad.Title',            N'Integridad de la base de datos', N'Database integrity'),
    (N'FrmAlertaIntegridad.aviso',            N'La verificación de integridad detectó diferencias en la base de datos. Restaure un backup confiable.', N'The integrity check found differences in the database. Restore a trusted backup.'),
    (N'FrmAlertaIntegridad.btnRestaurar',     N'Restaurar backup', N'Restore backup'),
    (N'FrmAlertaIntegridad.btnBackup',        N'Generar backup', N'Create backup'),
    (N'FrmAlertaIntegridad.confirmar',        N'Se reemplazará la base de datos actual por el backup ''{0}''. ¿Continuar?', N'The current database will be replaced by backup ''{0}''. Continue?'),
    (N'FrmAlertaIntegridad.tipo.DVH',         N'Registro modificado o sin dígito verificador', N'Row modified or missing check digit'),
    (N'FrmAlertaIntegridad.tipo.DVV',         N'Registros agregados o quitados', N'Rows added or removed'),
    (N'FrmAlertaIntegridad.tipo.NoVerificable', N'No se pudo verificar (base no disponible o incompleta)', N'Could not be verified (database unavailable or incomplete)'),
    (N'msg.backup.baseCorrupta',              N'No se puede generar un backup mientras la integridad de la base esté comprometida.', N'A backup cannot be created while database integrity is compromised.'),
    (N'msg.backup.generado',                  N'Backup generado: {0}', N'Backup created: {0}'),
    (N'FrmAlertaIntegridad.perdida',          N'Este backup es del {0}. Si lo restaura se perderán los cambios hechos desde esa fecha ({1} registros). Último cambio actual: {2}. ¿Restaurar igualmente?', N'This backup is from {0}. Restoring it will lose the changes made since then ({1} rows). Latest current change: {2}. Restore anyway?'),
    (N'msg.backup.invalido',                  N'El backup no es confiable: su contenido no supera la verificación de integridad o pertenece a otra base. No se restauró.', N'The backup is not trustworthy: its content fails the integrity check or belongs to another database. Nothing was restored.'),
    (N'msg.backup.perdidaDatos',              N'El backup es del {0} y no contiene {1} cambios posteriores. Confirme la pérdida para restaurarlo.', N'The backup is from {0} and lacks {1} later changes. Confirm the data loss to restore it.'),
    (N'msg.backup.noExiste',                N'El archivo de backup indicado no existe.', N'The backup file does not exist.'),
    (N'msg.backup.restaurado',                N'Backup restaurado. La integridad de la base es correcta.', N'Backup restored. Database integrity is valid.'),
    (N'msg.backup.restauradoConErrores',      N'Backup restaurado, pero la verificación de integridad sigue fallando. Pruebe con otro backup.', N'Backup restored, but the integrity check still fails. Try another backup.');

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
