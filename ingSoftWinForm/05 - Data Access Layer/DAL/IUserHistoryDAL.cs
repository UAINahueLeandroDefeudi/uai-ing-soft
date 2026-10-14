namespace DAL
{
    /// <summary>
    /// Historial por usuario (tabla [UserHistory]): ingresos, salidas, intentos
    /// fallidos y campos modificados. Solo se inserta, nunca se actualiza ni borra.
    /// </summary>
    public interface IUserHistoryDAL
    {
        void Insert(Guid? userId, string username, string eventType, bool success,
            int failedAttempts, string? detail = null,
            string? fieldName = null, string? oldValue = null, string? newValue = null);
    }
}
