using System.Security.Cryptography;
using System.Text;

namespace Services
{
    /// <summary>
    /// RNF-Seguridad-01: la contraseña se guarda sólo como hash + salt.
    /// La contraseña en claro nunca sale de esta clase ni viaja a la base de datos.
    /// </summary>
    public static class HashManager
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public static byte[] GenerateSalt()
            => RandomNumberGenerator.GetBytes(SaltSize);

        public static byte[] HashPassword(string password, byte[] salt)
            => Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

        public static bool VerifyPassword(string password, byte[] salt, byte[] hash)
        {
            var candidate = HashPassword(password, salt);
            // Comparación de tiempo fijo: no corta en el primer byte distinto,
            // así el tiempo de respuesta no filtra información del hash.
            return CryptographicOperations.FixedTimeEquals(candidate, hash);
        }
    }
}
