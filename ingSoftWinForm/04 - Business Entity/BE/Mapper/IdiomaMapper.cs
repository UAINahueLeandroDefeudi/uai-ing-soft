using System.Data;
using BE.Base;
using BE.Entity;

namespace BE.Mapper
{
    public class IdiomaMapper : BaseMapper<Idioma>
    {
        public override Idioma MapToEntity(DataRow row)
        {
            return new Idioma
            {
                Id = (int)row["IdIdioma"],
                Codigo = (string)row["Codigo"],
                Nombre = (string)row["Nombre"],
                EsDefault = (bool)row["EsDefault"],
                Activo = (bool)row["Activo"]
            };
        }
    }
}
