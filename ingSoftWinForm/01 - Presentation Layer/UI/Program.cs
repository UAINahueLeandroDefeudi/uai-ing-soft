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
            using var login = new FrmLogin();
            if (login.ShowDialog() != DialogResult.OK) return;

            Application.Run(new FrmMain());
        }
    }
}
