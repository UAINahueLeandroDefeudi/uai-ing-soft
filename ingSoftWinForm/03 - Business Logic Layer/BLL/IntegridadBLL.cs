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

            foreach (var dvh in dvhs.Where(d => !d.EsValido))
                resultado.Errores.Add(new ErrorIntegridad { Tabla = entidad.Tabla, Clave = dvh.Clave, Tipo = TipoErrorIntegridad.DVH });

            if (!calculador.VerificarIntegridadDVV([dvv]))
                resultado.Errores.Add(new ErrorIntegridad { Tabla = entidad.Tabla, Tipo = TipoErrorIntegridad.DVV });
        }
    }
}
