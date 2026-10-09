using System.Data;
using BE.Base;
using BE.Entity;

namespace BE.Mapper
{
    public class TraduccionItemMapper : BaseMapper<TraduccionItem>
    {
        public override TraduccionItem MapToEntity(DataRow row)
        {
            return new TraduccionItem
            {
                IdEtiqueta = (int)row["IdEtiqueta"],
                Clave = (string)row["Clave"],
                TextoDefault = row["TextoDefault"] as string ?? string.Empty,
                Texto = row["Texto"] as string
            };
        }
    }
}
