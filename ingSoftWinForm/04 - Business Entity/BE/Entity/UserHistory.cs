using BE.Enum;

namespace BE.Entity
{
    /// <summary>
    /// Fila de [User_History]: copia de los seis campos versionados del usuario (NickName, FirstName,
    /// LastName, Pais, Provincia, Direccion) tal como quedaron en esa versión. No guarda valor viejo /
    /// valor nuevo: lo que cambió se ve comparando dos versiones, y cualquier versión se puede volver a
    /// aplicar. Responde quién (EventBy), cuándo (EventAt) y qué. El Username de login no se versiona.
    /// </summary>
    public class UserHistory
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public int Version { get; set; }

        public string? NickName { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Pais { get; set; }
        public string? Provincia { get; set; }
        public string? Direccion { get; set; }

        public TipoCambioUsuario TipoCambio { get; set; }

        /// <summary>Si <see cref="TipoCambio"/> es Restauracion, número de la versión que se volvió a aplicar.</summary>
        public int? RestauradaDesde { get; set; }

        public DateTime EventAt { get; set; }

        /// <summary>Id del usuario que hizo el cambio.</summary>
        public Guid EventBy { get; set; }

        /// <summary>Username de <see cref="EventBy"/>, solo para mostrar (sale de un JOIN, no se guarda).</summary>
        public string EventByUsername { get; set; } = string.Empty;
    }
}
