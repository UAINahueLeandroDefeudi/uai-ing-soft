using BE.Enum;

namespace BE.Entity
{
    /// <summary>
    /// Registro de auditoría: qué pasó, quién lo hizo y con qué criticidad.
    /// Ver DER-login.md (entidad BITACORA) y RNF-Seguridad-03 del CU-01.
    ///
    /// No hereda de BaseEntity/BaseGuidEntity porque la PK se llama id_bitacora,
    /// ni de BaseAuditEntity porque la bitácora *es* la auditoría: sus filas se
    /// insertan una vez y no se modifican nunca.
    /// </summary>
    public class Bitacora
    {
        public int id_bitacora { get; set; }

        public BitacoraType Type { get; set; }
        public NameEvent NameEvent { get; set; }
        public Priority Priority { get; set; }
        public string Detail { get; set; } = string.Empty;
        public DateTime BitacoraDate { get; set; }

        // Foto del usuario en el instante del evento: se copian los datos en vez de
        // referenciar al [User] para que la traza sobreviva a una baja o un renombre.
        // Quedan vacíos cuando no hay usuario (login con un username inexistente).
        public string IdUser { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Roles y permisos que tenía el usuario en ese momento, aplanados a texto.
        /// </summary>
        public string RolesPermisos { get; set; } = string.Empty;

        public override string ToString() => $"[{Priority}] {NameEvent} - {Detail}";
    }
}
