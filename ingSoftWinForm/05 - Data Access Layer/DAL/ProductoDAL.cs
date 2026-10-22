using System.Data;
using BE.Entity;
using BE.Enum;
using BE.Mapper;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class ProductoDAL : IProductoDAL
    {
        private readonly DatabaseHelper dbHelper = new();
        private readonly DigitoVerificadorDAL integridad = new();
        private readonly ProductoMapper mapper = new();
        private readonly ProductoHistorialMapper historialMapper = new();

        /// <summary>Copia la fila actual del producto al historial: la versión y la fecha salen del propio producto.</summary>
        private const string InsertHistorial = @"
            INSERT INTO [Producto_Historial]
                ([ProductoId], [Version], [Codigo], [Nombre], [Descripcion], [Precio], [Stock], [Activo],
                 [Operacion], [EventAt], [EventBy])
            SELECT [Id], [Version], [Codigo], [Nombre], [Descripcion], [Precio], [Stock], [Activo],
                   @Operacion, [FechaModificacion], @EventBy
            FROM [Producto]
            WHERE [Id] = @Id;
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        public List<Producto> GetAll()
        {
            const string query = "SELECT * FROM [Producto] ORDER BY [Codigo]";
            return mapper.MapAll(dbHelper.ExecuteDataSet(query, CommandType.Text, []).Tables[0]).ToList();
        }

        public Producto? GetById(int id) => GetUno("[Id] = @P", new SqlParameter("@P", id));

        public Producto? GetByCodigo(string codigo) => GetUno("[Codigo] = @P", new SqlParameter("@P", codigo));

        public List<ProductoHistorial> GetHistorial(int productoId)
        {
            // LEFT JOIN: la versión se muestra aunque el usuario ya no exista.
            const string query = @"
                SELECT h.*, u.[Username] AS [EventByUsername]
                FROM [Producto_Historial] h
                LEFT JOIN [User] u ON u.[Id] = h.[EventBy]
                WHERE h.[ProductoId] = @ProductoId
                ORDER BY h.[Version] DESC";

            var ds = dbHelper.ExecuteDataSet(query, CommandType.Text, [new SqlParameter("@ProductoId", productoId)]);
            return historialMapper.MapAll(ds.Tables[0]).ToList();
        }

        public void Insert(Producto producto, Guid eventBy)
        {
            const string query = @"
                INSERT INTO [Producto]
                    ([Codigo], [Nombre], [Descripcion], [Precio], [Stock], [Activo], [Version], [FechaModificacion])
                OUTPUT inserted.[Id], inserted.[Version], inserted.[FechaModificacion]
                VALUES (@Codigo, @Nombre, @Descripcion, @Precio, @Stock, @Activo, 1, SYSDATETIME());";

            // T07: producto, historial y los DVH/DVV de ambas tablas se guardan juntos.
            integridad.Escribir((connection, transaction) =>
            {
                using (var command = new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddRange(Campos(producto));
                    using var reader = command.ExecuteReader();
                    reader.Read();
                    producto.Id = reader.GetInt32(0);
                    producto.Version = reader.GetInt32(1);
                    producto.FechaModificacion = reader.GetDateTime(2);
                }

                var historialId = RegistrarHistorial(connection, transaction, producto.Id, OperacionProducto.Alta, eventBy);

                return Tocadas(producto.Id, historialId);
            });
        }

        public bool Update(Producto producto, OperacionProducto operacion, Guid eventBy)
        {
            // La condición sobre Version evita pisar el cambio de otro usuario hecho después de leer.
            const string query = @"
                UPDATE [Producto]
                SET [Codigo] = @Codigo, [Nombre] = @Nombre, [Descripcion] = @Descripcion, [Precio] = @Precio,
                    [Stock] = @Stock, [Activo] = @Activo,
                    [Version] = [Version] + 1, [FechaModificacion] = SYSDATETIME()
                OUTPUT inserted.[Version], inserted.[FechaModificacion]
                WHERE [Id] = @Id AND [Version] = @Version;";

            var actualizado = false;

            integridad.Escribir((connection, transaction) =>
            {
                using (var command = new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddRange(Campos(producto));
                    command.Parameters.Add(new SqlParameter("@Id", producto.Id));
                    command.Parameters.Add(new SqlParameter("@Version", producto.Version));

                    using var reader = command.ExecuteReader();
                    if (!reader.Read()) return [];

                    producto.Version = reader.GetInt32(0);
                    producto.FechaModificacion = reader.GetDateTime(1);
                }

                var historialId = RegistrarHistorial(connection, transaction, producto.Id, operacion, eventBy);
                actualizado = true;

                return Tocadas(producto.Id, historialId);
            });

            return actualizado;
        }

        private static long RegistrarHistorial(SqlConnection connection, SqlTransaction transaction,
            int productoId, OperacionProducto operacion, Guid eventBy)
        {
            using var command = new SqlCommand(InsertHistorial, connection, transaction);
            command.Parameters.Add(new SqlParameter("@Id", productoId));
            command.Parameters.Add(new SqlParameter("@Operacion", operacion.ToString()));
            command.Parameters.Add(new SqlParameter("@EventBy", SqlDbType.UniqueIdentifier) { Value = eventBy });

            return Convert.ToInt64(command.ExecuteScalar());
        }

        private static IEnumerable<(EntidadVerificable, Dictionary<string, object?>?)> Tocadas(int productoId, long historialId)
            =>
            [
                (EntidadesVerificables.Producto, new Dictionary<string, object?> { ["Id"] = productoId }),
                (EntidadesVerificables.ProductoHistorial, new Dictionary<string, object?> { ["Id"] = historialId })
            ];

        private static SqlParameter[] Campos(Producto producto)
            =>
            [
                new SqlParameter("@Codigo", producto.Codigo),
                new SqlParameter("@Nombre", producto.Nombre),
                new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500) { Value = (object?)producto.Descripcion ?? DBNull.Value },
                new SqlParameter("@Precio", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = producto.Precio },
                new SqlParameter("@Stock", producto.Stock),
                new SqlParameter("@Activo", producto.Activo)
            ];

        private Producto? GetUno(string where, SqlParameter parameter)
        {
            var query = $"SELECT * FROM [Producto] WHERE {where}";
            return mapper.MapToEntity(dbHelper.ExecuteDataSet(query, CommandType.Text, [parameter]).Tables[0]);
        }
    }
}
