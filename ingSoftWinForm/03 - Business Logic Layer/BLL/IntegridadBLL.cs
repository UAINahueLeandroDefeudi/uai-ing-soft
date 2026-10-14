using BE.Entity;
using DAL;
using Services;

namespace BLL
{
    /// <summary>
    /// T07: verificación de integridad al iniciar. Recorre el registro de entidades
    /// verificables; agregar una tabla al registro la incorpora sin tocar esta clase.
    /// Es solo lectura: nunca recalcula ni guarda dígitos verificadores.
    /// </summary>
    public class IntegridadBLL
    {
        private readonly IDigitoVerificadorDAL dal;
        private readonly DigitoVerificador calculador = new();

        private readonly IReadOnlyList<EntidadVerificable> entidades;

        public IntegridadBLL() : this(new DigitoVerificadorDAL()) { }

        public IntegridadBLL(IDigitoVerificadorDAL dal) : this(dal, EntidadesVerificables.Todas) { }

        /// <summary>Permite verificar un conjunto distinto del registro (tests de genericidad).</summary>
        public IntegridadBLL(IDigitoVerificadorDAL dal, IReadOnlyList<EntidadVerificable> entidades)
        {
            this.dal = dal;
            this.entidades = entidades;
        }

        public IReadOnlyList<EntidadVerificable> Entidades => entidades;

        /// <summary>Verifica todas las tablas, guarda el resultado en <see cref="IntegridadManager"/> y lo devuelve.</summary>
        public ResultadoIntegridad Verificar()
        {
            var resultado = Verificar(dal);
            IntegridadManager.Establecer(resultado);
            return resultado;
        }

        /// <summary>
        /// Verifica otra fuente (p. ej. un backup montado) sin tocar <see cref="IntegridadManager"/>:
        /// el resultado global es el de la base en uso.
        /// </summary>
        public ResultadoIntegridad Verificar(IDigitoVerificadorDAL fuente)
        {
            var resultado = new ResultadoIntegridad();

            foreach (var entidad in entidades)
            {
                try
                {
                    VerificarEntidad(fuente, entidad, resultado);
                }
                catch (Exception ex)
                {
                    // BD caída o esquema incompleto: no se puede afirmar que haya corrupción.
                    System.Diagnostics.Debug.WriteLine(ex);
                    resultado.Errores.Add(new ErrorIntegridad
                    {
                        Tabla = entidad.Tabla,
                        Tipo = TipoErrorIntegridad.NoVerificable
                    });
                }
            }

            return resultado;
        }

        private void VerificarEntidad(IDigitoVerificadorDAL dal, EntidadVerificable entidad, ResultadoIntegridad resultado)
        {
            var filas = dal.LeerFilas(entidad);

            var dvhs = filas.Select(f => new DVH
            {
                Tabla = entidad.Tabla,
                Clave = calculador.ClaveTexto(entidad, f),
                Almacenado = f["DVH"] as string,
                Calculado = calculador.CalcularDVH(entidad, f)
            }).ToList();

            var dvv = new DVV
            {
                Tabla = entidad.Tabla,
                Almacenado = dal.LeerDVV(entidad.Tabla),
                Calculado = calculador.CalcularDVV(entidad.Tabla, dvhs.Select(d => (d.Clave, d.Almacenado)))
            };

            var dvhInvalidos = dvhs.Where(d => !d.EsValido).ToList();
            if (dvhInvalidos.Count == 0 && calculador.VerificarIntegridadDVV([dvv])) return;

            // Hay una falla: se reúne el detalle disponible (fecha de la fila y último cálculo legítimo del DVV).
            var ultimoCalculo = LeerFechaCalculo(dal, entidad.Tabla);

            var fechas = new Dictionary<string, DateTime?>();
            for (var i = 0; i < filas.Count; i++)
                if (filas[i].TryGetValue("EventAt", out var fecha) && fecha is DateTime evento)
                    fechas[dvhs[i].Clave] = evento;

            foreach (var dvh in dvhInvalidos)
                resultado.Errores.Add(new ErrorIntegridad
                {
                    Tabla = entidad.Tabla,
                    Clave = dvh.Clave,
                    Tipo = TipoErrorIntegridad.DVH,
                    FechaRegistro = fechas.GetValueOrDefault(dvh.Clave),
                    UltimoCalculoValido = ultimoCalculo
                });

            if (!calculador.VerificarIntegridadDVV([dvv]))
                resultado.Errores.Add(new ErrorIntegridad
                {
                    Tabla = entidad.Tabla,
                    Tipo = TipoErrorIntegridad.DVV,
                    UltimoCalculoValido = ultimoCalculo
                });
        }

        /// <summary>El detalle es accesorio: si no se puede leer, la falla se informa igual.</summary>
        private static DateTime? LeerFechaCalculo(IDigitoVerificadorDAL dal, string tabla)
        {
            try { return dal.LeerFechaCalculo(tabla); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }
    }
}
