using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using BE.Entity;

namespace Services
{
    /// <summary>
    /// T07: cálculo y verificación de dígitos verificadores, genérico para cualquier
    /// <see cref="EntidadVerificable"/>. No conoce SQL ni la UI.
    /// Formato canónico (UTF-16LE, el mismo que arma sql/08 con HASHBYTES):
    ///   DVH = SHA256( Tabla | i:longitud:valor | i:N (null) ... )   i = posición del atributo, desde 1
    ///   DVV = SHA256( Tabla | N | clave=DVH | clave=DVH ... )        ordenado por clave (ordinal)
    /// </summary>
    public class DigitoVerificador
    {
        public string CalcularDVH(EntidadVerificable entidad, IReadOnlyDictionary<string, object?> fila)
        {
            var sb = new StringBuilder(entidad.Tabla);

            for (var i = 0; i < entidad.Atributos.Count; i++)
            {
                var valor = Formatear(fila[entidad.Atributos[i]]);
                sb.Append('|').Append(i + 1).Append(':');
                if (valor == null) sb.Append('N');
                else sb.Append(valor.Length).Append(':').Append(valor);
            }

            return Hash(sb.ToString());
        }

        /// <summary>Texto de la PK de la fila (valores unidos por ':').</summary>
        public string ClaveTexto(EntidadVerificable entidad, IReadOnlyDictionary<string, object?> fila)
            => string.Join(':', entidad.Clave.Select(c => Formatear(fila[c]) ?? string.Empty));

        /// <summary>DVV a partir de (clave, DVH almacenado) de todas las filas de la tabla.</summary>
        public string CalcularDVV(string tabla, IEnumerable<(string Clave, string? DVH)> filas)
        {
            var ordenadas = filas.OrderBy(f => f.Clave, StringComparer.Ordinal).ToList();
            var sb = new StringBuilder(tabla).Append('|').Append(ordenadas.Count);

            foreach (var (clave, dvh) in ordenadas)
                sb.Append('|').Append(clave).Append('=').Append(dvh ?? string.Empty);

            return Hash(sb.ToString());
        }

        /// <summary>true si todos los DVH almacenados coinciden con los recalculados.</summary>
        public bool VerificarIntegridadDVH(List<DVH> dvhs) => dvhs.All(d => d.EsValido);

        /// <summary>true si todos los DVV almacenados coinciden con los recalculados.</summary>
        public bool VerificarIntegridadDVV(List<DVV> dvvs) => dvvs.All(d => d.EsValido);

        /// <summary>
        /// true si el DVV almacenado no corresponde a las filas actuales: la tabla se alteró fuera de la
        /// aplicación. Una tabla vacía sin DVV no cuenta. Quien escribe NO debe recalcular el DVV de una
        /// tabla en este estado: lo daría por válido y taparía la alteración.
        /// </summary>
        public bool EstaAdulterada(string tabla, string? dvvAlmacenado, IEnumerable<(string Clave, string? DVH)> filas)
        {
            var lista = filas.ToList();
            if (dvvAlmacenado == null && lista.Count == 0) return false;

            return !string.Equals(dvvAlmacenado, CalcularDVV(tabla, lista), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Valor de una columna tal como entra al cálculo (para mostrarlo y compararlo).</summary>
        public static string? Texto(object? valor) => Formatear(valor);

        private static string? Formatear(object? valor) => valor switch
        {
            null => null,
            DBNull => null,
            bool b => b ? "1" : "0",
            Guid g => g.ToString("D").ToUpperInvariant(),
            DateTime d => d.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => valor.ToString()
        };

        private static string Hash(string canonico)
            => Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(canonico)));
    }
}
