namespace BE.Entity
{
    /// <summary>
    /// Resultado de una operación de negocio que puede ser rechazada por una regla
    /// (como LoginResult en el login): los casos esperables no se informan con excepciones.
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; private init; }
        public string Message { get; private init; } = string.Empty;

        public static OperationResult Ok(string message = "") => new() { Success = true, Message = message };

        public static OperationResult Fail(string message) => new() { Success = false, Message = message };
    }
}
