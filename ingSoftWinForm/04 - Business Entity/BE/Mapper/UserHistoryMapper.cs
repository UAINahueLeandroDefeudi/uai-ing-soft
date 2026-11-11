using System.Data;
using BE.Base;
using BE.Entity;
using BE.Enum;

namespace BE.Mapper
{
    public class UserHistoryMapper : BaseMapper<UserHistory>
    {
        public override UserHistory MapToEntity(DataRow row)
        {
            return new UserHistory
            {
                Id = (long)row["Id"],
                UserId = (Guid)row["UserId"],
                Version = (int)row["Version"],
                NickName = row["NickName"] as string,
                FirstName = (string)row["FirstName"],
                LastName = (string)row["LastName"],
                Pais = row["Pais"] as string,
                Provincia = row["Provincia"] as string,
                Direccion = row["Direccion"] as string,
                TipoCambio = System.Enum.Parse<TipoCambioUsuario>((string)row["TipoCambio"]),
                RestauradaDesde = row["RestauradaDesde"] as int?,
                EventAt = (DateTime)row["EventAt"],
                EventBy = (Guid)row["EventBy"],
                EventByUsername = row["EventByUsername"] as string ?? string.Empty
            };
        }
    }
}
