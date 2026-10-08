using System.Data;
using BE.Base;
using BE.Entity;

namespace BE.Mapper
{
    /// <summary>
    /// Fábrica del Composite: según [Permission].[IsCompound] instancia la hoja o el compuesto.
    /// Sólo mapea la fila; los hijos del compuesto los arma la DAL.
    /// </summary>
    public class PermissionMapper : BaseMapper<Permission>
    {
        public override Permission MapToEntity(DataRow row)
        {
            Permission permission = (bool)row["IsCompound"]
                ? new CompoundPermission()
                : new SimplePermission();

            permission.Id = (int)row["Id"];
            permission.Code = (string)row["Code"];
            permission.Name = (string)row["Name"];
            return permission;
        }
    }
}
