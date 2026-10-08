#!/usr/bin/env bash
#
# Asigna (o quita) un rol a un usuario existente de IF_DB (T04).
#
# Caso tipico: un usuario se registro desde la app (rol 'invitado') y hay que
# darle el rol 'administrador' para que pueda abrir "Gestion de roles".
# Tambien sirve para crear el primer administrador, que no puede salir de la app
# porque asignar roles ya requiere ser administrador.
#
# Es idempotente: asignar un rol que el usuario ya tiene no hace nada.
# Requiere haber corrido sql/04_init_create_roles_permission.sql (o init-db.sh).
#
# Uso:
#   ./sql/set-user-role.sh -u pepe                       # le da el rol 'administrador'
#   ./sql/set-user-role.sh -u pepe -r moderador          # le da otro rol
#   ./sql/set-user-role.sh -u pepe -r invitado -x        # le quita ese rol
#
set -euo pipefail

USERNAME=""
ROLE="administrador"
REMOVE=0
SERVER='localhost\SQLEXPRESS'
DATABASE="IF_DB"

usage() {
    cat <<'USAGE'
Asigna o quita un rol a un usuario existente en [dbo].[User_Role].

Uso:
  ./set-user-role.sh -u <username> [opciones]

Obligatorio:
  -u <username>    Usuario existente

Opcionales:
  -r <rol>         Rol (default: administrador). Debe existir en [dbo].[Role]
  -x               Quita el rol en lugar de asignarlo
  -S <servidor>    Instancia SQL   (default: localhost\SQLEXPRESS)
  -d <base>        Base de datos   (default: IF_DB)
  -h               Muestra esta ayuda

Roles sembrados: invitado, client, moderador, administrador.
Los cambios se ven en el proximo inicio de sesion del usuario.
USAGE
}

while getopts ":u:r:xS:d:h" opt; do
    case "$opt" in
        u) USERNAME="$OPTARG" ;;
        r) ROLE="$OPTARG" ;;
        x) REMOVE=1 ;;
        S) SERVER="$OPTARG" ;;
        d) DATABASE="$OPTARG" ;;
        h) usage; exit 0 ;;
        :)  echo "Error: la opcion -$OPTARG necesita un valor." >&2; echo >&2; usage >&2; exit 2 ;;
        \?) echo "Error: opcion desconocida -$OPTARG." >&2; echo >&2; usage >&2; exit 2 ;;
    esac
done

if [[ -z "$USERNAME" ]]; then
    echo "Error: -u (username) es obligatorio." >&2
    echo >&2
    usage >&2
    exit 2
fi

if ! command -v sqlcmd >/dev/null 2>&1; then
    echo "Error: no se encontro 'sqlcmd' en el PATH." >&2
    echo "       Suele estar en: /c/Program Files/Microsoft SQL Server/Client SDK/ODBC/170/Tools/Binn" >&2
    exit 1
fi

# --- Las tablas de roles tienen que existir (las crea init-db.sh / el script 04) ---
HAS_ROLES="$(sqlcmd -S "$SERVER" -E -C -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID('$DATABASE') IS NOT NULL AND OBJECT_ID('[$DATABASE].[dbo].[Role]', 'U') IS NOT NULL AND OBJECT_ID('[$DATABASE].[dbo].[User_Role]', 'U') IS NOT NULL THEN 1 ELSE 0 END" 2>/dev/null | tr -d '[:space:]' || true)"
if [[ "$HAS_ROLES" != "1" ]]; then
    echo "Error: no existen las tablas de roles en [$DATABASE] (o no se pudo conectar a $SERVER)." >&2
    echo "       Crealas primero con:  ./sql/init-db.sh" >&2
    exit 1
fi

# --- Escape de comillas simples para no romper los literales SQL ---
sql_str() { printf "'%s'" "${1//\'/\'\'}"; }

u_sql="$(sql_str "$USERNAME")"
r_sql="$(sql_str "$ROLE")"

if [[ "$REMOVE" -eq 1 ]]; then
    ACTION_SQL="DELETE FROM [dbo].[User_Role] WHERE [UserId] = @UserId AND [RoleId] = @RoleId;"
    VERB="quitado"
else
    ACTION_SQL="IF NOT EXISTS (SELECT 1 FROM [dbo].[User_Role] WHERE [UserId] = @UserId AND [RoleId] = @RoleId)
        INSERT INTO [dbo].[User_Role] ([UserId], [RoleId]) VALUES (@UserId, @RoleId);"
    VERB="asignado"
fi

# --- Script temporal (sqlcmd necesita ruta Windows) ---
TMP_SQL="$(mktemp --suffix=.sql)"
cleanup() { rm -f "$TMP_SQL"; }
trap cleanup EXIT

cat > "$TMP_SQL" <<SQL
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @UserId UNIQUEIDENTIFIER = (SELECT [Id] FROM [dbo].[User] WHERE [Username] = $u_sql);
DECLARE @RoleId INT              = (SELECT [Id] FROM [dbo].[Role] WHERE [Name]     = $r_sql);

IF @UserId IS NULL
    RAISERROR('No existe un usuario con ese username', 16, 1);
ELSE IF @RoleId IS NULL
    RAISERROR('No existe un rol con ese nombre (ver [dbo].[Role])', 16, 1);
ELSE
BEGIN
    $ACTION_SQL

    SELECT u.[Username], r.[Name] AS [Rol]
    FROM [dbo].[User] u
    LEFT JOIN [dbo].[User_Role] ur ON ur.[UserId] = u.[Id]
    LEFT JOIN [dbo].[Role]      r  ON r.[Id] = ur.[RoleId]
    WHERE u.[Id] = @UserId
    ORDER BY r.[Id];
END
SQL

if command -v cygpath >/dev/null 2>&1; then
    TMP_SQL_WIN="$(cygpath -w "$TMP_SQL")"
else
    TMP_SQL_WIN="$TMP_SQL"
fi

echo "Rol '$ROLE' sobre el usuario '$USERNAME' en $SERVER / $DATABASE ..."

# -I fuerza QUOTED_IDENTIFIER ON (indice filtrado de [User]). -b hace que el RAISERROR devuelva exit code != 0.
if ! sqlcmd -S "$SERVER" -E -C -I -b -W -d "$DATABASE" -i "$TMP_SQL_WIN"; then
    echo "Fallo la operacion." >&2
    exit 1
fi

echo
echo "OK. Rol '$ROLE' $VERB. Roles actuales del usuario listados arriba."
