namespace BE.Entity
{
    /// <summary>
    /// Resultado de una operación de negocio que puede ser rechazada por una regla
    /// (como LoginResult en el login): los casos esperables no se informan con excepciones.
    /// T05: la BLL no devuelve texto sino la clave de la etiqueta y sus argumentos;
    /// la UI la traduce al idioma activo.
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; private init; }

        /// <summary>Clave de la etiqueta del mensaje (p. ej. "msg.role.created"); vacía si no hay mensaje.</summary>
        public string MessageKey { get; private init; } = string.Empty;

        /// <summary>Valores para los marcadores {0}, {1}... del texto traducido.</summary>
        public object[] MessageArgs { get; private init; } = Array.Empty<object>();

        public static OperationResult Ok(string messageKey = "", params object[] args)
            => new() { Success = true, MessageKey = messageKey, MessageArgs = args };

        public static OperationResult Fail(string messageKey, params object[] args)
            => new() { Success = false, MessageKey = messageKey, MessageArgs = args };
    }
}
