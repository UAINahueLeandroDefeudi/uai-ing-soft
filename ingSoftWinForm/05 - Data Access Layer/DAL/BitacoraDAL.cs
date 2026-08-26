using System.Data;
using BE.Entity;
using BE.Enum;
using BE.Mapper;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class BitacoraDAL : IBitacoraDAL
    {
        private readonly DatabaseHelper dbHelper;
        private readonly BitacoraMapper mapper;

        public BitacoraDAL()
        {
            dbHelper = new DatabaseHelper();
            mapper = new BitacoraMapper();
        }

        /// <summary>
        /// Inserta el registro y devuelve el id_bitacora generado.
        /// Los tres enums viajan por nombre (ToString), no por ordinal.
        /// </summary>
        public int Insert(Bitacora bitacora)
        {
            const string query = @"
                INSERT INTO [Bitacora]
                    ([Type], [NameEvent], [Priority], [Detail], [BitacoraDate],
                     [IdUser], [Email], [FirstName], [LastName], [RolesPermisos])
                VALUES
                    (@Type, @NameEvent, @Priority, @Detail, @BitacoraDate,
                     @IdUser, @Email, @FirstName, @LastName, @RolesPermisos);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters =
            [
                new SqlParameter("@Type", bitacora.Type.ToString()),
                new SqlParameter("@NameEvent", bitacora.NameEvent.ToString()),
                new SqlParameter("@Priority", bitacora.Priority.ToString()),
                new SqlParameter("@Detail", bitacora.Detail),
                new SqlParameter("@BitacoraDate", bitacora.BitacoraDate),
                new SqlParameter("@IdUser", bitacora.IdUser),
                new SqlParameter("@Email", bitacora.Email),
                new SqlParameter("@FirstName", bitacora.FirstName),
                new SqlParameter("@LastName", bitacora.LastName),
                new SqlParameter("@RolesPermisos", bitacora.RolesPermisos)
            ];

            var id = dbHelper.ExecuteScalar(query, CommandType.Text, parameters);
            bitacora.id_bitacora = id == null || id == DBNull.Value ? 0 : (int)id;

            return bitacora.id_bitacora;
        }

        /// <summary>Lo más reciente primero, que es como se mira una bitácora.</summary>
        public List<Bitacora> GetAll()
        {
            const string query = "SELECT * FROM [Bitacora] ORDER BY id_bitacora DESC";
            DataSet ds = dbHelper.ExecuteDataSet(query, CommandType.Text, []);
            return mapper.MapAll(ds.Tables[0]).ToList();
        }

        /// <summary>
        /// El filtro se resuelve en el motor y no en memoria: la bitacora crece sin
        /// techo y traerla entera para descartar en el cliente no escala.
        /// El patron "@P IS NULL OR col = @P" evita armar el WHERE concatenando texto.
        /// </summary>
        public List<Bitacora> GetByFilter(DateTime from, DateTime to,
            BitacoraType? type, NameEvent? nameEvent, Priority? priority)
        {
            const string query = @"
                SELECT * FROM [Bitacora]
                WHERE [BitacoraDate] >= @From
                  AND [BitacoraDate] <  @To
                  AND (@Type      IS NULL OR [Type]      = @Type)
                  AND (@NameEvent IS NULL OR [NameEvent] = @NameEvent)
                  AND (@Priority  IS NULL OR [Priority]  = @Priority)
                ORDER BY id_bitacora DESC";

            SqlParameter[] parameters =
            [
                new SqlParameter("@From", from),
                new SqlParameter("@To", to),
                Opcional("@Type", type),
                Opcional("@NameEvent", nameEvent),
                Opcional("@Priority", priority)
            ];

            DataSet ds = dbHelper.ExecuteDataSet(query, CommandType.Text, parameters);
            return mapper.MapAll(ds.Tables[0]).ToList();
        }

        /// <summary>
        /// Enum opcional como NVARCHAR. El tipo va explicito porque con DBNull.Value
        /// SqlParameter no tiene de donde inferirlo.
        /// </summary>
        private static SqlParameter Opcional<TEnum>(string nombre, TEnum? valor)
            where TEnum : struct, System.Enum
            => new SqlParameter(nombre, SqlDbType.NVarChar, 30)
            {
                Value = valor.HasValue ? valor.Value.ToString() : DBNull.Value
            };
    }
}
