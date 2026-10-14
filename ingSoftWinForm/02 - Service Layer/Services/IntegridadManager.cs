using BE.Entity;

namespace Services
{
    /// <summary>
    /// T07: guarda el resultado de la verificación hecha al arrancar para que el login
    /// decida qué mostrar según el rol (el rol se conoce recién después de autenticar).
    /// </summary>
    public static class IntegridadManager
    {
        public static ResultadoIntegridad? Resultado { get; private set; }

        /// <summary>true si se verificó y hay problemas. Sin verificación previa no bloquea.</summary>
        public static bool HayProblemas => Resultado is { EsValido: false };

        public static void Establecer(ResultadoIntegridad? resultado) => Resultado = resultado;
    }
}
