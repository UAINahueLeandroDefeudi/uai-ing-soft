using BE.Entity;

namespace DAL
{
    public interface IIdiomaDAL
    {
        List<Idioma> GetAll();
        Idioma? GetById(int id);
        Idioma? GetByCodigo(string codigo);
        Idioma? GetDefault();

        /// <summary>Alta de un idioma (queda activo y no default). Devuelve el id generado.</summary>
        int Insert(Idioma idioma);

        /// <summary>Modifica nombre y estado activo. El código y el default no cambian.</summary>
        void Update(Idioma idioma);

        /// <summary>Clave -> texto de todas las etiquetas traducidas a ese idioma (una sola consulta).</summary>
        Dictionary<string, string> GetTextos(int idIdioma);

        /// <summary>Todas las etiquetas con su texto por defecto y su traducción (null si falta).</summary>
        List<TraduccionItem> GetTraducciones(int idIdioma);

        /// <summary>Crea o actualiza la traducción; con texto vacío la elimina (vuelve a "sin traducir").</summary>
        void UpsertTraduccion(int idEtiqueta, int idIdioma, string? texto);

        void SetUserIdioma(Guid userId, int idIdioma);
    }
}
