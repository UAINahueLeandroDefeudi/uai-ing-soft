using System.Data;
using BE.Base;
using BE.Entity;

namespace BE.Mapper
{
    /// <summary>Mapea sólo la fila del rol; sus permisos los arma la DAL (ver RoleDAL).</summary>
    public class RoleMapper : BaseMapper<Role>
    {
        public override Role MapToEntity(DataRow row)
        {
            return new Role
            {
                Id = (int)row["Id"],
                Name = (string)row["Name"],
                CreatedAt = (DateTime)row["CreatedAt"],
                CreatedBy = row["CreatedBy"] as string,
                UpdatedAt = row["UpdatedAt"] as DateTime?,
                UpdatedBy = row["UpdatedBy"] as string
            };
        }
    }
}
