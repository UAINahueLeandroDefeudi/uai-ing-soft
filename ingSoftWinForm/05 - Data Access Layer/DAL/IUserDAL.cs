using BE.Entity;

namespace DAL
{
    public interface IUserDAL
    {
        User? GetByUsername(string username);
        List<User> GetAll();
        bool Block(string username);
        void IncrementFailedAttempts(string username);
        void ResetFailedAttempts(Guid id);
    }
}
