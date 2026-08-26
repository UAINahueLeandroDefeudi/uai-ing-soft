using BE.Entity;
using BE.Enum;

namespace DAL
{
    public interface IBitacoraDAL
    {
        int Insert(Bitacora bitacora);
        List<Bitacora> GetAll();

        /// <summary>
        /// Rango de fechas obligatorio; los tres enums son opcionales y un null
        /// significa "no filtrar por esa columna".
        /// </summary>
        List<Bitacora> GetByFilter(DateTime from, DateTime to,
            BitacoraType? type, NameEvent? nameEvent, Priority? priority);
    }
}
