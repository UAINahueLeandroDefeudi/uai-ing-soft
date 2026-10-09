using System.Data;
using BE.Entity;
using BE.Mapper;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class IdiomaDAL : IIdiomaDAL
    {
        private readonly DatabaseHelper dbHelper;
        private readonly IdiomaMapper mapper;
        private readonly TraduccionItemMapper traduccionMapper;

        public IdiomaDAL()
        {
            dbHelper = new DatabaseHelper();
            mapper = new IdiomaMapper();
            traduccionMapper = new TraduccionItemMapper();
        }

        public List<Idioma> GetAll()
        {
            const string query = "SELECT * FROM [Idioma] ORDER BY [EsDefault] DESC, [Nombre]";
            DataSet ds = dbHelper.ExecuteDataSet(query, CommandType.Text, []);
            return mapper.MapAll(ds.Tables[0]).ToList();
        }

        public Idioma? GetById(int id) => GetUno("[IdIdioma] = @P", new SqlParameter("@P", id));

        public Idioma? GetByCodigo(string codigo) => GetUno("[Codigo] = @P", new SqlParameter("@P", codigo));

        public Idioma? GetDefault() => GetUno("[EsDefault] = 1");

        public int Insert(Idioma idioma)
        {
            const string query = @"
                INSERT INTO [Idioma] ([Codigo], [Nombre], [EsDefault], [Activo])
                VALUES (@Codigo, @Nombre, 0, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters =
            [
                new SqlParameter("@Codigo", idioma.Codigo),
                new SqlParameter("@Nombre", idioma.Nombre)
            ];

            idioma.Id = Convert.ToInt32(dbHelper.ExecuteScalar(query, CommandType.Text, parameters));
            idioma.EsDefault = false;
            idioma.Activo = true;
            return idioma.Id;
        }

        public void Update(Idioma idioma)
        {
            const string query = "UPDATE [Idioma] SET [Nombre] = @Nombre, [Activo] = @Activo WHERE [IdIdioma] = @Id";
            SqlParameter[] parameters =
            [
                new SqlParameter("@Nombre", idioma.Nombre),
                new SqlParameter("@Activo", idioma.Activo),
                new SqlParameter("@Id", idioma.Id)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public Dictionary<string, string> GetTextos(int idIdioma)
        {
            const string query = @"
                SELECT e.[Clave], t.[Texto]
                FROM [Traduccion] t
                JOIN [Etiqueta] e ON e.[IdEtiqueta] = t.[IdEtiqueta]
                WHERE t.[IdIdioma] = @IdIdioma";

            DataSet ds = dbHelper.ExecuteDataSet(query, CommandType.Text, [new SqlParameter("@IdIdioma", idIdioma)]);

            var textos = new Dictionary<string, string>();
            foreach (DataRow row in ds.Tables[0].Rows)
                textos[(string)row["Clave"]] = (string)row["Texto"];

            return textos;
        }

        public List<TraduccionItem> GetTraducciones(int idIdioma)
        {
            const string query = @"
                SELECT e.[IdEtiqueta], e.[Clave], d.[Texto] AS [TextoDefault], t.[Texto]
                FROM [Etiqueta] e
                LEFT JOIN [Traduccion] d ON d.[IdEtiqueta] = e.[IdEtiqueta]
                                        AND d.[IdIdioma] = (SELECT [IdIdioma] FROM [Idioma] WHERE [EsDefault] = 1)
                LEFT JOIN [Traduccion] t ON t.[IdEtiqueta] = e.[IdEtiqueta] AND t.[IdIdioma] = @IdIdioma
                ORDER BY e.[Clave]";

            DataSet ds = dbHelper.ExecuteDataSet(query, CommandType.Text, [new SqlParameter("@IdIdioma", idIdioma)]);
            return traduccionMapper.MapAll(ds.Tables[0]).ToList();
        }

        public void UpsertTraduccion(int idEtiqueta, int idIdioma, string? texto)
        {
            const string query = @"
                IF @Texto IS NULL
                    DELETE FROM [Traduccion] WHERE [IdEtiqueta] = @IdEtiqueta AND [IdIdioma] = @IdIdioma;
                ELSE IF EXISTS (SELECT 1 FROM [Traduccion] WHERE [IdEtiqueta] = @IdEtiqueta AND [IdIdioma] = @IdIdioma)
                    UPDATE [Traduccion] SET [Texto] = @Texto WHERE [IdEtiqueta] = @IdEtiqueta AND [IdIdioma] = @IdIdioma;
                ELSE
                    INSERT INTO [Traduccion] ([IdEtiqueta], [IdIdioma], [Texto]) VALUES (@IdEtiqueta, @IdIdioma, @Texto);";

            SqlParameter[] parameters =
            [
                new SqlParameter("@IdEtiqueta", idEtiqueta),
                new SqlParameter("@IdIdioma", idIdioma),
                new SqlParameter("@Texto", SqlDbType.NVarChar, 500)
                {
                    Value = string.IsNullOrWhiteSpace(texto) ? DBNull.Value : texto
                }
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        public void SetUserIdioma(Guid userId, int idIdioma)
        {
            const string query = "UPDATE [User] SET [IdIdioma] = @IdIdioma, [UpdatedAt] = SYSDATETIME() WHERE [Id] = @Id";
            SqlParameter[] parameters =
            [
                new SqlParameter("@IdIdioma", idIdioma),
                new SqlParameter("@Id", userId)
            ];

            dbHelper.ExecuteNonQuery(query, CommandType.Text, parameters);
        }

        private Idioma? GetUno(string where, params SqlParameter[] parameters)
        {
            DataSet ds = dbHelper.ExecuteDataSet($"SELECT * FROM [Idioma] WHERE {where}", CommandType.Text, parameters);
            return ds.Tables[0].Rows.Count > 0 ? mapper.MapToEntity(ds.Tables[0].Rows[0]) : null;
        }
    }
}
