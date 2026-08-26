using BE.Entity;

namespace Services
{
    /// <summary>
    /// Patrón Singleton: existe una y sólo una sesión activa mientras la aplicación
    /// está en ejecución. Ver DC-sesion-singleton.md.
    /// </summary>
    public class SessionManager
    {
        private static SessionManager? _session;

        public User User { get; private set; } = null!;
        public DateTime StartedAt { get; private set; }

        private SessionManager() { }

        public static SessionManager GetInstance
        {
            get
            {
                if (_session == null) throw new InvalidOperationException("Sesión no iniciada");
                return _session;
            }
        }

        public static bool IsLoggedIn() => _session != null;

        public static void Login(User user)
        {
            if (_session != null) throw new InvalidOperationException("Sesión ya iniciada");

            _session = new SessionManager
            {
                User = user,
                StartedAt = DateTime.Now
            };
        }

        public static void Logout()
        {
            if (_session == null) throw new InvalidOperationException("Sesión no iniciada");
            _session = null;
        }
    }
}
