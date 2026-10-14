using System.Data;
using BE.Base;
using BE.Entity;

namespace BE.Mapper
{
    /// <summary>
    /// Fábrica del Composite: según [Permission].[IsFamily] e [IsRol] instancia la hoja,
    /// el compuesto o el rol. Es el único lugar que decide el tipo, mediante una tabla
    /// (sin if). El CHECK de la base impide la combinación (hoja, rol).
    /// Sólo mapea la fila; los hijos del compuesto los carga la DAL con LoadChildren.
    /// </summary>
    public class PermissionMapper : BaseMapper<Permission>
    {
        private static readonly Dictionary<(bool IsFamily, bool IsRol), Func<Permission>> Factory = new()
        {
            [(false, false)] = () => new SimplePermission(),
            [(true, false)] = () => new CompoundPermission(),
            [(true, true)] = () => new RolePermission()
        };

        public override Permission MapToEntity(DataRow row)
        {
            var permission = Factory[((bool)row["IsFamily"], (bool)row["IsRol"])]();

            permission.Id = (int)row["Id"];
            permission.Code = (string)row["Code"];
            permission.Name = (string)row["Name"];
            return permission;
        }
    }
}
