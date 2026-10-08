#!/usr/bin/env bash
#
# Inicializa IF_DB: crea la base y las tablas SOLO si no existen.
#
# Es seguro de re-ejecutar: nunca borra datos. En particular NO vuelve a correr
# 01_create_table_User.sql si [User] ya existe, porque ese script hace DROP TABLE.
#
#   [User]                          01_create_table_User.sql        (solo si falta)
#   [Bitacora]                      03_create_table_Bitacora.sql    (solo si falta)
#   [Permission], [Role], ...       04_init_create_roles_permission.sql
#                                   (idempotente: crea lo que falta y resiembra sin duplicar)
#
# Uso:
#   ./sql/init-db.sh
#   ./sql/init-db.sh -S 'localhost\SQLEXPRESS'
#
# Requisito: SQL Server en ejecucion y sqlcmd en el PATH (ver README, "Quick start").
#
set -euo pipefail

SERVER='localhost\SQLEXPRESS'
DATABASE="IF_DB"

usage() {
    cat <<'USAGE'
Crea la base IF_DB y las tablas que falten. No borra datos existentes.

Uso:
  ./init-db.sh [opciones]

Opcionales:
  -S <servidor>    Instancia SQL   (default: localhost\SQLEXPRESS)
  -h               Muestra esta ayuda

Los scripts .sql tienen "USE [IF_DB]" fijo: la base se llama siempre IF_DB.
USAGE
}

while getopts ":S:h" opt; do
    case "$opt" in
        S) SERVER="$OPTARG" ;;
        h) usage; exit 0 ;;
        :)  echo "Error: la opcion -$OPTARG necesita un valor." >&2; exit 2 ;;
        \?) echo "Error: opcion desconocida -$OPTARG." >&2; echo >&2; usage >&2; exit 2 ;;
    esac
done

if ! command -v sqlcmd >/dev/null 2>&1; then
    echo "Error: no se encontro 'sqlcmd' en el PATH." >&2
    echo "       Suele estar en: /c/Program Files/Microsoft SQL Server/Client SDK/ODBC/170/Tools/Binn" >&2
    exit 1
fi

# Carpeta de los .sql: la del propio script, sin importar desde donde se lo llame.
SQL_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

to_win() { if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else printf '%s' "$1"; fi; }

# -I: QUOTED_IDENTIFIER ON (lo exige el indice filtrado UX_User_Email). -C: confiar en el certificado.
run_sql()   { sqlcmd -S "$SERVER" -E -C -I -b "$@"; }
run_file()  { run_sql -d "$DATABASE" -i "$(to_win "$SQL_DIR/$1")"; }
scalar()    { run_sql -h -1 -W -Q "SET NOCOUNT ON; $1" | tr -d '[:space:]'; }

# --- 1. Conexion ---
echo "Conectando a $SERVER ..."
if ! run_sql -Q "SELECT 1" >/dev/null 2>&1; then
    echo "Error: no se pudo conectar a '$SERVER'." >&2
    echo "       Verifique que el servicio de SQL Server este iniciado:" >&2
    echo "         PowerShell (admin):  Start-Service 'MSSQL\$SQLEXPRESS'" >&2
    echo "       y que el nombre de la instancia sea el correcto (-S)." >&2
    exit 1
fi
echo "  OK."

# --- 2. Base ---
if [[ "$(scalar "SELECT CASE WHEN DB_ID('$DATABASE') IS NULL THEN 0 ELSE 1 END")" == "1" ]]; then
    echo "Base [$DATABASE]: ya existe."
else
    echo "Base [$DATABASE]: creando ..."
    run_sql -Q "CREATE DATABASE [$DATABASE];"
fi

table_exists() {
    [[ "$(scalar "SELECT CASE WHEN DB_ID('$DATABASE') IS NOT NULL AND OBJECT_ID('[$DATABASE].[dbo].[$1]', 'U') IS NOT NULL THEN 1 ELSE 0 END")" == "1" ]]
}

# --- 3. Tablas ---
if table_exists "User"; then
    echo "Tabla [User]: ya existe (no se toca)."
else
    echo "Tabla [User]: creando ..."
    run_file "01_create_table_User.sql" >/dev/null
fi

if table_exists "Bitacora"; then
    echo "Tabla [Bitacora]: ya existe (no se toca)."
else
    echo "Tabla [Bitacora]: creando ..."
    run_file "03_create_table_Bitacora.sql" >/dev/null
fi

# Roles y permisos (T04): el script es idempotente, se corre siempre para completar lo que falte.
echo "Roles y permisos: creando lo que falte y sembrando el catalogo ..."
run_file "04_init_create_roles_permission.sql" >/dev/null

echo
echo "Listo. Estado de IF_DB:"
run_sql -d "$DATABASE" -W -Q "SET NOCOUNT ON;
SELECT 'User'       AS [Tabla], COUNT(*) AS [Filas] FROM [dbo].[User]
UNION ALL SELECT 'Bitacora',   COUNT(*) FROM [dbo].[Bitacora]
UNION ALL SELECT 'Permission', COUNT(*) FROM [dbo].[Permission]
UNION ALL SELECT 'Role',       COUNT(*) FROM [dbo].[Role]
UNION ALL SELECT 'User_Role',  COUNT(*) FROM [dbo].[User_Role];"

echo
echo "Siguiente paso: crear un usuario y darle un rol, por ejemplo:"
echo "  ./sql/create-user.sh -u admin -p Admin123 -r administrador"
