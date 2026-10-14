namespace BE.Entity
{
    /// <summary>Cambios de la base actual que un backup no contiene (y que se perderían al restaurarlo).</summary>
    public class PerdidaTabla
    {
        public string Tabla { get; set; } = string.Empty;

        /// <summary>Filas que existen hoy y no están en el backup.</summary>
        public int FilasQueSePierden { get; set; }

        /// <summary>Filas que existen en ambos pero con otro contenido (DVH distinto).</summary>
        public int FilasModificadas { get; set; }

        /// <summary>Filas que están en el backup y hoy faltan (se recuperarían al restaurar).</summary>
        public int FilasQueSeRecuperan { get; set; }

        /// <summary>Cambios de la base actual que se perderían (no incluye lo que se recupera).</summary>
        public int Total => FilasQueSePierden + FilasModificadas;
    }

    /// <summary>T07: diagnóstico de un .bkp antes de restaurarlo (la base real no se modifica).</summary>
    public class ResultadoValidacionBackup
    {
        /// <summary>Clave de etiqueta si la validación no pudo hacerse (sin permiso, no existe, otra base, error).</summary>
        public string? ErrorKey { get; set; }

        public DateTime FechaBackup { get; set; }

        /// <summary>Integridad (DVH/DVV) del contenido del backup.</summary>
        public ResultadoIntegridad Integridad { get; set; } = new();

        /// <summary>Último evento registrado hoy en la base actual (null si no hay fecha).</summary>
        public DateTime? UltimoCambioActual { get; set; }

        public List<PerdidaTabla> Perdidas { get; } = new();

        /// <summary>Detalle fila por fila (y columna por columna) de lo que difiere de la base actual.</summary>
        public List<DiferenciaBackup> Diferencias { get; } = new();

        /// <summary>Los cambios hechos desde esta fecha no están en el backup.</summary>
        public DateTime PerdidaDesde => FechaBackup;

        public int TotalPerdido => Perdidas.Sum(p => p.Total);

        public bool HayPerdida => TotalPerdido > 0;
    }
}
