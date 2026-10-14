using BE.Entity;

namespace DAL
{
    /// <summary>
    /// T07: lectura genérica de las tablas verificables. Las filas vuelven como
    /// columna -> valor (incluye "DVH"); el cálculo lo hace la BLL con Services.
    /// </summary>
    public interface IDigitoVerificadorDAL
    {
        List<Dictionary<string, object?>> LeerFilas(EntidadVerificable entidad);

        /// <summary>DVV guardado en [DigitoVerificador]; null si la tabla no tiene control.</summary>
        string? LeerDVV(string tabla);
    }
}
