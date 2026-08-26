/* ============================================================
   CU-01 Iniciar sesion - Usuario de prueba
   Base: IF_DB   (ejecutar despues de 01_create_table_User.sql)
   ============================================================

   ESTE SCRIPT NO CREA USUARIOS. Solo lista los que hay.

   Para crear uno, usar:

       ./create-user.sh -u admin -p Admin123

   Por que no se puede sembrar un usuario desde un .sql suelto:
   la contrasena se guarda como PBKDF2-SHA256 con 100.000 iteraciones
   (Services.HashManager). T-SQL no tiene PBKDF2 -- HASHBYTES hace un SHA2_256
   de una sola pasada, que no es lo mismo. Un hash escrito a mano hace que
   HashManager.VerifyPassword devuelva siempre false y el login no entre nunca.

   create-user.sh calcula el mismo PBKDF2 (via python) y hace el INSERT.
   Ver ./create-user.sh -h para todas las opciones.

   ------------------------------------------------------------
   Si igual se quiere hacer el INSERT a mano, primero generar el par:

       python -c "
       import hashlib, secrets
       salt = secrets.token_bytes(16)
       h = hashlib.pbkdf2_hmac('sha256', b'Admin123', salt, 100000, 32)
       print('SALT = 0x' + salt.hex().upper())
       print('HASH = 0x' + h.hex().upper())"

   y despues descomentar el INSERT de abajo pegando los dos literales 0x...

   OJO: la tabla tiene un indice filtrado (UX_User_Email), asi que todo
   INSERT/UPDATE sobre [User] exige QUOTED_IDENTIFIER ON. Desde SSMS ya viene
   en ON; desde sqlcmd hay que pasar el flag -I o falla con el error 1934.
   ------------------------------------------------------------

   Ejecucion:
       sqlcmd -S localhost\SQLEXPRESS -E -C -I -d IF_DB -i sql\02_seed_User.sql
   ============================================================ */

USE [IF_DB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- INSERT INTO [dbo].[User]
--     ([Username], [PasswordHash], [Salt], [FirstName], [LastName], [Email], [CreatedBy])
-- VALUES
--     ('admin',
--      0x<<PEGAR_HASH_AQUI>>,   -- 32 bytes
--      0x<<PEGAR_SALT_AQUI>>,   -- 16 bytes
--      'Admin', 'Del Sistema', 'admin@if.local', 'seed');
-- GO

SELECT [Id], [Username], [FirstName], [LastName], [FailedAttempts], [IsBlocked], [IsActive], [LastLoginAt]
FROM [dbo].[User];
GO
