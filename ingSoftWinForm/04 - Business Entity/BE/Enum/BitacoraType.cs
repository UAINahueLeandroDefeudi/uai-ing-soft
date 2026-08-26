namespace BE.Enum
{
    /// <summary>
    /// Naturaleza del registro de bitácora. Se corresponde con las dos fábricas
    /// de Services.BitacoraManager: EventoBitacora y ErrorBitacora.
    /// </summary>
    public enum BitacoraType
    {
        Event,
        Error
    }
}
