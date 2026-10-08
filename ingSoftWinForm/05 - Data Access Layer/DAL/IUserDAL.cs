using BE.Entity;

namespace DAL
{
    public interface IUserDAL
    {
        User? GetByUsername(string username);
        List<User> GetAll();
        bool EmailExists(string email);
        bool Block(string username);
        void IncrementFailedAttempts(string username);
        void ResetFailedAttempts(Guid id);

        /// <summary>
        /// Alta de usuario con su rol inicial en una sola transacción (T04: registro con rol 'invitado').
        /// </summary>
        void Insert(User user, string roleName);
    }
}
