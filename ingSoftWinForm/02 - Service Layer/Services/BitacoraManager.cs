using BE.Entity;
using BE.Enum;

namespace Services
{
    /// <summary>
    /// Fábrica de registros de bitácora (Servicios.Bitacora del diagrama de clases).
    /// Sólo *construye* la entidad: la capa de servicios no referencia a la DAL, así
    /// que la persistencia queda en BLL.BitacoraBLL.
    /// </summary>
    public static class BitacoraManager
    {
        /// <summary>Tope de [Bitacora].[Detail]. Ver sql/03_create_table_Bitacora.sql.</summary>
        private const int MaxDetailLength = 500;

        public static Bitacora EventoBitacora(NameEvent nameEvent, string detail, Priority priority, User user)
            => Crear(BitacoraType.Event, nameEvent, detail, priority, user);

        public static Bitacora ErrorBitacora(NameEvent nameEvent, string detail, Priority priority, User user)
            => Crear(BitacoraType.Error, nameEvent, detail, priority, user);

        /// <summary>
        /// Toma el usuario de la sesión activa. Si todavía no hay sesión abierta
        /// —el caso de un login fallido— los datos de usuario quedan vacíos.
        /// </summary>
        public static Bitacora EventoBitacora(NameEvent nameEvent, string detail, Priority priority)
            => Crear(BitacoraType.Event, nameEvent, detail, priority, UsuarioDeSesion());

        /// <inheritdoc cref="EventoBitacora(NameEvent, string, Priority)"/>
        public static Bitacora ErrorBitacora(NameEvent nameEvent, string detail, Priority priority)
            => Crear(BitacoraType.Error, nameEvent, detail, priority, UsuarioDeSesion());

        private static Bitacora Crear(BitacoraType type, NameEvent nameEvent, string detail, Priority priority, User? user)
        {
            var bitacora = new Bitacora
            {
                Type = type,
                NameEvent = nameEvent,
                Priority = priority,
                Detail = Recortar(detail),
                BitacoraDate = DateTime.Now
            };

            if (user == null) return bitacora;

            bitacora.IdUser = user.Id.ToString();
            bitacora.Email = user.Email ?? string.Empty;
            bitacora.FirstName = user.FirstName;
            bitacora.LastName = user.LastName;
            bitacora.RolesPermisos = AplanarRolesPermisos(user);

            return bitacora;
        }

        /// <summary>
        /// El detalle puede traer texto tipeado por el usuario (por ejemplo el username
        /// de un login fallido), que no tiene tope de longitud en la UI.
        /// </summary>
        private static string Recortar(string detail)
            => detail.Length <= MaxDetailLength ? detail : detail[..MaxDetailLength];

        private static User? UsuarioDeSesion()
            => SessionManager.IsLoggedIn() ? SessionManager.GetInstance.User : null;

        /// <summary>
        /// Deja por escrito los roles y permisos que tenía el usuario en ese momento.
        /// TODO: completar cuando esté implementado el árbol Composite de permisos
        /// (ver DC-permisos-composite.md). Hasta entonces devuelve vacío.
        /// </summary>
        private static string AplanarRolesPermisos(User user) => string.Empty;
    }
}
