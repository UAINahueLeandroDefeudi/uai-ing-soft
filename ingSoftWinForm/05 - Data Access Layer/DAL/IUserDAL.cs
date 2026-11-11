using BE.Entity;
using BE.Enum;

namespace DAL
{
    public interface IUserDAL
    {
        User? GetByUsername(string username);
        User? GetById(Guid id);
        List<User> GetAll();
        bool EmailExists(string email);
        bool Block(string username);
        void IncrementFailedAttempts(string username);
        void ResetFailedAttempts(Guid id);

        /// <summary>
        /// Alta de usuario con su rol inicial en una sola transacción (T04: registro con rol 'invitado').
        /// También deja la versión 1 (Alta) en [User_History].
        /// </summary>
        void Insert(User user, string roleName);

        /// <summary>
        /// Control de cambios: versiones del usuario, de la más vieja a la más nueva, con el username
        /// de quien generó cada una.
        /// </summary>
        List<UserHistory> GetHistory(Guid userId);

        /// <summary>
        /// Guarda los seis campos versionados del usuario (NickName, FirstName, LastName, Pais, Provincia,
        /// Direccion) como una versión nueva (Version + 1) y la copia a [User_History] con
        /// <paramref name="tipo"/> (y <paramref name="restauradaDesde"/> si es una restauración), todo en
        /// una transacción. Devuelve false, sin escribir nada, si el usuario ya no está en la versión que se
        /// leyó (otro usuario lo modificó antes). Si escribe, actualiza <c>user.Version</c>.
        /// </summary>
        bool Update(User user, TipoCambioUsuario tipo, Guid eventBy, int? restauradaDesde = null);
    }
}
