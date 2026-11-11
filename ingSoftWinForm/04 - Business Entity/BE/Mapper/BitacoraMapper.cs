using System.Data;
using BE.Base;
using BE.Entity;
using BE.Enum;

namespace BE.Mapper
{
    public class BitacoraMapper : BaseMapper<Bitacora>
    {
        public override Bitacora MapToEntity(DataRow row)
        {
            return new Bitacora
            {
                id_bitacora = (int)row["id_bitacora"],
                // Los tres enums se guardan por nombre y no por ordinal: la bitácora
                // se consulta con un SELECT suelto y 'Critical' se lee, 4 no.
                Type = System.Enum.Parse<BitacoraType>((string)row["Type"]),
                // Un evento que ya no existe en el enum (fila vieja) se lee como EventoObsoleto.
                NameEvent = System.Enum.TryParse<NameEvent>((string)row["NameEvent"], out var evento)
                    ? evento
                    : NameEvent.EventoObsoleto,
                Priority = System.Enum.Parse<Priority>((string)row["Priority"]),
                Detail = (string)row["Detail"],
                BitacoraDate = (DateTime)row["BitacoraDate"],
                IdUser = (string)row["IdUser"],
                Email = (string)row["Email"],
                FirstName = (string)row["FirstName"],
                LastName = (string)row["LastName"],
                RolesPermisos = (string)row["RolesPermisos"],
                Roles = (string)row["Roles"]
            };
        }
    }
}
