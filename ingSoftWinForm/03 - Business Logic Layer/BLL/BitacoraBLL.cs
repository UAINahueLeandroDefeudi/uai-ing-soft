using BE.Entity;
using BE.Enum;
using DAL;
using Services;

namespace BLL
{
    /// <summary>
    /// Persiste los registros de auditoría. RNF-Seguridad-03: todo intento de
    /// acceso queda auditado. Ver BitacoraBLL en DC-login.md.
    /// </summary>
    public class BitacoraBLL
    {
        private readonly IBitacoraDAL bitacoraDAL;

        public BitacoraBLL() : this(new BitacoraDAL()) { }

        public BitacoraBLL(IBitacoraDAL bitacoraDAL)
        {
            this.bitacoraDAL = bitacoraDAL;
        }

        /// <summary>
        /// Nunca propaga: que falle la auditoría no puede voltear la operación que
        /// se estaba auditando. Si se cayó la base, el login igual tiene que poder
        /// devolver su LoginResult (CU-01, FA-5).
        /// </summary>
        public void Registrar(Bitacora bitacora)
        {
            try
            {
                bitacoraDAL.Insert(bitacora);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo registrar en bitácora: {ex}");
            }
        }

        /// <summary>
        /// Registra un evento. Sin <paramref name="user"/> toma el de la sesión activa.
        /// </summary>
        public void RegistrarEvento(NameEvent nameEvent, string detail, Priority priority, User? user = null)
            => Registrar(user == null
                ? BitacoraManager.EventoBitacora(nameEvent, detail, priority)
                : BitacoraManager.EventoBitacora(nameEvent, detail, priority, user));

        /// <inheritdoc cref="RegistrarEvento"/>
        public void RegistrarError(NameEvent nameEvent, string detail, Priority priority, User? user = null)
            => Registrar(user == null
                ? BitacoraManager.ErrorBitacora(nameEvent, detail, priority)
                : BitacoraManager.ErrorBitacora(nameEvent, detail, priority, user));

        public List<Bitacora> GetAll() => bitacoraDAL.GetAll();

        /// <summary>
        /// Rango inclusivo por día: <paramref name="to"/> se lleva al final de su jornada
        /// para que un filtro "del 1 al 1" traiga los registros de todo ese día.
        /// Los tres enums son opcionales; null es "no filtrar por esa columna".
        /// </summary>
        public List<Bitacora> GetByFilter(DateTime from, DateTime to,
            BitacoraType? type = null, NameEvent? nameEvent = null, Priority? priority = null)
            => bitacoraDAL.GetByFilter(from.Date, to.Date.AddDays(1), type, nameEvent, priority);
    }
}
