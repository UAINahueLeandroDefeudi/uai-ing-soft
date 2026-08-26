#!/usr/bin/env bash
#
# Crea un usuario en IF_DB para probar el CU-01 Login.
#
# Calcula el par salt/hash con PBKDF2-SHA256 y 100.000 iteraciones, que es
# exactamente lo que hace Services.HashManager, y lo inserta en [dbo].[User].
#
# Existe porque PBKDF2 no se puede calcular en T-SQL: un .sql suelto nunca
# puede sembrar un usuario valido. HASHBYTES hace un SHA2_256 de una sola
# pasada, que no es lo mismo. Si se inserta un hash escrito a mano,
# HashManager.VerifyPassword devuelve siempre false y el login no entra nunca.
#
# Uso:
#   ./create-user.sh -u admin -p Admin123
#   ./create-user.sh -u pepe -p Test1234 -f Jose -l Perez -e pepe@if.local
#
set -euo pipefail

# --- Mismos parametros que Services.HashManager ---
readonly SALT_SIZE=16
readonly HASH_SIZE=32
readonly ITERATIONS=100000

USERNAME=""
PASSWORD=""
FIRST_NAME="Nombre"
LAST_NAME="Apellido"
EMAIL=""
SERVER='localhost\SQLEXPRESS'
DATABASE="IF_DB"

usage() {
    cat <<'USAGE'
Crea un usuario en [dbo].[User] con el hash que espera Services.HashManager.

Uso:
  ./create-user.sh -u <username> -p <password> [opciones]

Obligatorios:
  -u <username>    Nombre de usuario (unico)
  -p <password>    Contrasena en claro. Se guarda solo como PBKDF2 + salt.

Opcionales:
  -f <nombre>      FirstName          (default: Nombre)
  -l <apellido>    LastName           (default: Apellido)
  -e <email>       Email              (default: NULL)
  -S <servidor>    Instancia SQL      (default: localhost\SQLEXPRESS)
  -d <base>        Base de datos      (default: IF_DB)
  -h               Muestra esta ayuda

Ejemplos:
  ./create-user.sh -u admin -p Admin123 -f Admin -l "Del Sistema" -e admin@if.local
  ./create-user.sh -u pepe -p Test1234 -f Jose -l Perez
USAGE
}

while getopts ":u:p:f:l:e:S:d:h" opt; do
    case "$opt" in
        u) USERNAME="$OPTARG" ;;
        p) PASSWORD="$OPTARG" ;;
        f) FIRST_NAME="$OPTARG" ;;
        l) LAST_NAME="$OPTARG" ;;
        e) EMAIL="$OPTARG" ;;
        S) SERVER="$OPTARG" ;;
        d) DATABASE="$OPTARG" ;;
        h) usage; exit 0 ;;
        :)  echo "Error: la opcion -$OPTARG necesita un valor." >&2; echo >&2; usage >&2; exit 2 ;;
        \?) echo "Error: opcion desconocida -$OPTARG." >&2; echo >&2; usage >&2; exit 2 ;;
    esac
done

if [[ -z "$USERNAME" || -z "$PASSWORD" ]]; then
    echo "Error: -u (username) y -p (password) son obligatorios." >&2
    echo >&2
    usage >&2
    exit 2
fi

# --- Dependencias ---
if ! command -v python >/dev/null 2>&1; then
    echo "Error: hace falta 'python' para calcular el PBKDF2." >&2
    echo "       openssl 1.1.1 no sirve: el subcomando 'kdf' recien aparece en 3.0." >&2
    exit 1
fi

if ! command -v sqlcmd >/dev/null 2>&1; then
    echo "Error: no se encontro 'sqlcmd' en el PATH." >&2
    echo "       Suele estar en: /c/Program Files/Microsoft SQL Server/Client SDK/ODBC/170/Tools/Binn" >&2
    exit 1
fi

# --- Salt aleatorio + hash, en un solo llamado ---
# La contrasena va por variable de entorno y no por argumento: los argumentos
# de un proceso son visibles para cualquier otro proceso de la maquina.
read -r SALT_HEX HASH_HEX < <(
    CU_PASSWORD="$PASSWORD" python -c '
import hashlib, os, secrets, sys
salt_size, hash_size, iterations = (int(a) for a in sys.argv[1:4])
salt = secrets.token_bytes(salt_size)
pwd  = os.environ["CU_PASSWORD"].encode("utf-8")
hash = hashlib.pbkdf2_hmac("sha256", pwd, salt, iterations, hash_size)
print(salt.hex().upper(), hash.hex().upper())
' "$SALT_SIZE" "$HASH_SIZE" "$ITERATIONS"
)

# --- Escape de comillas simples para no romper los literales SQL ---
sql_str() { printf "'%s'" "${1//\'/\'\'}"; }

u_sql="$(sql_str "$USERNAME")"
f_sql="$(sql_str "$FIRST_NAME")"
l_sql="$(sql_str "$LAST_NAME")"
if [[ -z "$EMAIL" ]]; then
    e_sql="NULL"
else
    e_sql="$(sql_str "$EMAIL")"
fi

# --- Script temporal (sqlcmd necesita ruta Windows) ---
TMP_SQL="$(mktemp --suffix=.sql)"
cleanup() { rm -f "$TMP_SQL"; }
trap cleanup EXIT

cat > "$TMP_SQL" <<SQL
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF EXISTS (SELECT 1 FROM [dbo].[User] WHERE [Username] = $u_sql)
BEGIN
    RAISERROR('Ya existe un usuario con ese username', 16, 1);
END
ELSE
BEGIN
    INSERT INTO [dbo].[User]
        ([Username], [PasswordHash], [Salt], [FirstName], [LastName], [Email], [CreatedBy])
    VALUES
        ($u_sql, 0x$HASH_HEX, 0x$SALT_HEX, $f_sql, $l_sql, $e_sql, 'seed');
END
SQL

if command -v cygpath >/dev/null 2>&1; then
    TMP_SQL_WIN="$(cygpath -w "$TMP_SQL")"
else
    TMP_SQL_WIN="$TMP_SQL"
fi

echo "Creando usuario '$USERNAME' en $SERVER / $DATABASE ..."

# -I fuerza QUOTED_IDENTIFIER ON: lo exige el indice filtrado UX_User_Email.
# -b hace que sqlcmd devuelva exit code != 0 cuando dispara el RAISERROR.
if ! sqlcmd -S "$SERVER" -E -C -I -b -d "$DATABASE" -i "$TMP_SQL_WIN"; then
    echo "Fallo la creacion del usuario." >&2
    exit 1
fi

echo "OK. Usuario '$USERNAME' creado."
echo
sqlcmd -S "$SERVER" -E -C -I -W -d "$DATABASE" \
    -Q "SET NOCOUNT ON; SELECT Username, FirstName, LastName, FailedAttempts, IsBlocked, IsActive FROM [dbo].[User];"
