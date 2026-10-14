using System.Data;
using BE.Entity;
using Microsoft.Data.SqlClient;
using Services;

namespace DAL
{
    /// <summary>
    /// T07: único lugar con SQL de dígitos verificadores. Sirve para cualquier
    /// <see cref="EntidadVerificable"/>: tabla y columnas salen del descriptor.
    /// Además de leer, expone <see cref="Escribir"/>: las DAL de las tablas protegidas
    /// la usan para que el dato, su DVH y el DVV de la tabla cambien en una sola transacción.
    /// </summary>
    public class DigitoVerificadorDAL : IDigitoVerificadorDAL
    {
        private readonly DatabaseHelper dbHelper;
        private readonly DigitoVerificador calculador = new();

        public DigitoVerificadorDAL() : this(null) { }

        /// <summary>Con <paramref name="catalogo"/> lee otra base (la temporal de validación de un backup).</summary>
        public DigitoVerificadorDAL(string? catalogo) => dbHelper = new DatabaseHelper(catalogo);

        public List<Dictionary<string, object?>> LeerFilas(EntidadVerificable entidad)
        {
            var columnas = string.Join(", ", entidad.Atributos.Append("DVH").Select(c => $"[{c}]"));
            var query = $"SELECT {columnas} FROM [{entidad.Tabla}]";

            var tabla = dbHelper.ExecuteDataSet(query, CommandType.Text, []).Tables[0];

            return tabla.Rows.Cast<DataRow>()
                .Select(row => tabla.Columns.Cast<DataColumn>()
                    .ToDictionary(c => c.ColumnName, c => row[c] is DBNull ? null : row[c]))
                .ToList();
        }

        public string? LeerDVV(string tabla)
        {
            const string query = "SELECT DVV FROM [DigitoVerificador] WHERE Tabla = @Tabla";
            SqlParameter[] parameters = [new SqlParameter("@Tabla", tabla)];

            return dbHelper.ExecuteScalar(query, CommandType.Text, parameters) as string;
        }

        /// <summary>
        /// Corre <paramref name="escritura"/> en una transacción y después, en la misma,
        /// recalcula el DVH de las filas que devolvió y el DVV de las tablas tocadas.
        /// La escritura devuelve las claves (columna -> valor) de las filas altas o modificadas;
        /// una baja se informa con clave null (solo cambia el DVV).
        /// </summary>
        public void Escribir(
            Func<SqlConnection, SqlTransaction, IEnumerable<(EntidadVerificable Entidad, Dictionary<string, object?>? Clave)>> escritura)
        {
            dbHelper.ExecuteTransaction((connection, transaction) =>
            {
                var tocadas = escritura(connection, transaction).ToList();

                foreach (var (entidad, clave) in tocadas.Where(t => t.Clave != null))
                    RefrescarDVH(connection, transaction, entidad, clave!);

                foreach (var entidad in tocadas.Select(t => t.Entidad).Distinct())
                    RecalcularDVV(connection, transaction, entidad);

                return 0;
            });
        }

        private void RefrescarDVH(SqlConnection connection, SqlTransaction transaction,
            EntidadVerificable entidad, Dictionary<string, object?> clave)
        {
            var columnas = string.Join(", ", entidad.Atributos.Select(c => $"[{c}]"));
            var where = string.Join(" AND ", entidad.Clave.Select(c => $"[{c}] = @{c}"));

            Dictionary<string, object?>? fila = null;
            using (var select = new SqlCommand($"SELECT {columnas} FROM [{entidad.Tabla}] WHERE {where}", connection, transaction))
            {
                foreach (var c in entidad.Clave) select.Parameters.AddWithValue($"@{c}", clave[c] ?? DBNull.Value);

                using var reader = select.ExecuteReader();
                if (reader.Read())
                    fila = Enumerable.Range(0, reader.FieldCount)
                        .ToDictionary(i => reader.GetName(i), i => reader.IsDBNull(i) ? null : reader.GetValue(i));
            }

            if (fila == null) return;

            using var update = new SqlCommand($"UPDATE [{entidad.Tabla}] SET DVH = @DVH WHERE {where}", connection, transaction);
            update.Parameters.AddWithValue("@DVH", calculador.CalcularDVH(entidad, fila));
            foreach (var c in entidad.Clave) update.Parameters.AddWithValue($"@{c}", clave[c] ?? DBNull.Value);
            update.ExecuteNonQuery();
        }

        /// <summary>DVV sobre los DVH almacenados: una alta externa (DVH null) o una baja lo cambian.</summary>
        private void RecalcularDVV(SqlConnection connection, SqlTransaction transaction, EntidadVerificable entidad)
        {
            var columnas = string.Join(", ", entidad.Clave.Append("DVH").Select(c => $"[{c}]"));
            var filas = new List<(string Clave, string? DVH)>();

            using (var select = new SqlCommand($"SELECT {columnas} FROM [{entidad.Tabla}]", connection, transaction))
            using (var reader = select.ExecuteReader())
            {
                while (reader.Read())
                {
                    var fila = Enumerable.Range(0, reader.FieldCount)
                        .ToDictionary(i => reader.GetName(i), i => reader.IsDBNull(i) ? null : reader.GetValue(i));
                    filas.Add((calculador.ClaveTexto(entidad, fila), fila["DVH"] as string));
                }
            }

            const string merge = @"
                MERGE [DigitoVerificador] AS t
                USING (SELECT @Tabla AS Tabla) AS s ON t.Tabla = s.Tabla
                WHEN MATCHED THEN UPDATE SET DVV = @DVV, FechaCalculo = SYSDATETIME()
                WHEN NOT MATCHED THEN INSERT (Tabla, DVV, FechaCalculo) VALUES (@Tabla, @DVV, SYSDATETIME());";

            using var command = new SqlCommand(merge, connection, transaction);
            command.Parameters.AddWithValue("@Tabla", entidad.Tabla);
            command.Parameters.AddWithValue("@DVV", calculador.CalcularDVV(entidad.Tabla, filas));
            command.ExecuteNonQuery();
        }
    }
}
