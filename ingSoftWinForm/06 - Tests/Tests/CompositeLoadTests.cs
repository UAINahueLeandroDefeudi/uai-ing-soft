using System.Data;
using BE.Entity;
using BE.Mapper;
using Xunit;

namespace Tests
{
    /// <summary>Carga polimórfica del Composite: nadie pregunta el tipo, cada clase resuelve.</summary>
    public class CompositeLoadTests
    {
        private static DataRow Row(int id, string code, bool isFamily, bool isRol)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Code", typeof(string));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("IsFamily", typeof(bool));
            table.Columns.Add("IsRol", typeof(bool));
            table.Rows.Add(id, code, code, isFamily, isRol);
            return table.Rows[0];
        }

        [Theory]
        [InlineData(false, false, typeof(SimplePermission))]
        [InlineData(true, false, typeof(CompoundPermission))]
        [InlineData(true, true, typeof(RolePermission))]
        public void Mapper_ChoosesClass_FromFamilyAndRolBits(bool isFamily, bool isRol, Type expected)
        {
            var permission = new PermissionMapper().MapToEntity(Row(1, "X", isFamily, isRol));

            Assert.IsType(expected, permission);
            Assert.Equal(isRol, permission.IsRol);
        }

        [Fact]
        public void LoadChildren_OnEveryNode_BuildsNestedTree_LeafIgnoresIt()
        {
            var hoja = new SimplePermission { Id = 1, Code = "HOJA" };
            var familia = new CompoundPermission { Id = 2, Code = "FAM" };
            var rol = new RolePermission { Id = 3, Code = "ROL" };
            var catalog = new Permission[] { hoja, familia, rol };

            // ROL -> FAM -> HOJA
            var relations = new[] { (Parent: 3, Child: familia), (Parent: 2, Child: (Permission)hoja) }
                .ToLookup(r => r.Parent, r => r.Child);

            foreach (var permission in catalog) permission.LoadChildren(relations);

            Assert.Empty(hoja.GetChildren());
            Assert.True(rol.Grants("HOJA"));
            Assert.Equal(new[] { "HOJA" }, rol.Flatten().Select(p => p.Code));
        }
    }
}
