using UI.Login;

namespace UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // CU-01: sin sesión iniciada no se entra al menú principal.
            // Al cerrar sesión se vuelve al login; cualquier otra salida (Salir, cancelar el login) termina la app.
            while (true)
            {
                // T07: antes de cada login se verifica la integridad de la base (también tras un cierre
                // de sesión: la base pudo alterarse con la app abierta). No bloquea acá:
                // el resultado se aplica en el login según los permisos de backup.
                new BLL.IntegridadBLL().Verificar();

                using (var login = new FrmLogin())
                {
                    if (login.ShowDialog() != DialogResult.OK) return;
                }

                using var main = new FrmMain();
                Application.Run(main);

                if (!main.SesionCerrada) return;
            }
        }
    }
}
