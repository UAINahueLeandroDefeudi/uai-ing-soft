using System.Globalization;
using System.Text.RegularExpressions;
using BE.Entity;
using BE.Enum;
using BE.Observer;
using DAL;
using Services;

namespace BLL
{
    /// <summary>
    /// Gestión de idiomas (T05) y publisher del Observer: mantiene el idioma activo y avisa
    /// a los suscriptores (formularios) cuando cambia. No conoce la UI: los suscriptores
    /// son <see cref="ISuscriberIdioma"/>. Los textos viven en la base (Etiqueta/Traduccion),
    /// no en recursos estáticos, así que se pueden incorporar idiomas y leyendas en ejecución.
    /// Hay una instancia compartida (<see cref="Instance"/>) porque todos los formularios
    /// tienen que suscribirse al mismo publisher.
    /// </summary>
    public class IdiomaBLL : IPublisherIdioma
    {
        private static readonly Lazy<IdiomaBLL> instance = new(() => new IdiomaBLL());
        public static IdiomaBLL Instance => instance.Value;

        private static readonly Regex CodigoValido = new("^[a-z]{2,3}(-[a-z]{2,4})?$", RegexOptions.Compiled);
        private const int MaxNombreLength = 50;
        private const int MaxTextoLength = 500;

        private readonly IIdiomaDAL idiomaDAL;
        private readonly BitacoraBLL bitacoraBLL;
        private readonly List<ISuscriberIdioma> _suscribers = new();

        private Idioma? activo;
        private Idioma? porDefecto;
        private Dictionary<string, string> textosActivo = new();
        private Dictionary<string, string> textosDefault = new();
        private bool cargado;

        public IdiomaBLL() : this(new IdiomaDAL(), new BitacoraBLL()) { }

        public IdiomaBLL(IIdiomaDAL idiomaDAL, BitacoraBLL bitacoraBLL)
        {
            this.idiomaDAL = idiomaDAL;
            this.bitacoraBLL = bitacoraBLL;
        }

        // ---- Observer (publisher) ----

        public IReadOnlyList<ISuscriberIdioma> suscribers => _suscribers.AsReadOnly();

        public void suscribe(ISuscriberIdioma suscriber)
        {
            if (_suscribers.Contains(suscriber))
                throw new Exception($"Ya existe una suscripción para {suscriber}");

            _suscribers.Add(suscriber);
        }

        public void unsuscribe(ISuscriberIdioma suscriber)
        {
            if (!_suscribers.Remove(suscriber))
                throw new Exception($"No existe una suscripción para {suscriber}");
        }

        /// <summary>Se recorre una copia: un suscriptor puede desuscribirse (cerrarse) al ser notificado.</summary>
        public void notifySuscribers()
        {
            if (activo == null) return;

            foreach (var suscriber in _suscribers.ToList())
                suscriber.Actualizar(activo);
        }

        // ---- Idioma activo y traducción ----

        /// <summary>Idioma activo; si todavía no se eligió ninguno es el idioma por defecto. Null si no hay base.</summary>
        public Idioma? IdiomaActivo
        {
            get { Asegurar(); return activo; }
        }

        /// <summary>
        /// Resuelve una etiqueta en el idioma activo. Si no está traducida cae al idioma por
        /// defecto con Traducido=false; si no existe en ninguno devuelve la clave con Existe=false.
        /// Nunca lanza: sin base de datos la UI sigue mostrando sus textos de diseño.
        /// </summary>
        public Leyenda Traducir(string clave, params object[] args)
        {
            Asegurar();

            if (textosActivo.TryGetValue(clave, out var texto))
                return Armar(clave, texto, traducido: true, existe: true, args);

            if (textosDefault.TryGetValue(clave, out var textoDefault))
            {
                // Si el activo ES el default, el texto está "traducido" por definición.
                var esDefault = activo != null && porDefecto != null && activo.Id == porDefecto.Id;
                return Armar(clave, textoDefault, traducido: esDefault, existe: true, args);
            }

            return Armar(clave, clave, traducido: false, existe: false, args);
        }

        public Leyenda Traducir(OperationResult resultado) => Traducir(resultado.MessageKey, resultado.MessageArgs);

        public List<Idioma> GetIdiomas() => idiomaDAL.GetAll();

        public List<Idioma> GetIdiomasActivos() => idiomaDAL.GetAll().Where(i => i.Activo).ToList();

        /// <summary>
        /// Cambia el idioma activo, lo guarda en el usuario si hay sesión y avisa a los suscriptores.
        /// </summary>
        public OperationResult CambiarIdioma(string codigo)
        {
            try
            {
                var idioma = idiomaDAL.GetByCodigo(codigo);
                if (idioma == null || !idioma.Activo)
                    return OperationResult.Fail("msg.idioma.noDisponible", codigo);

                if (SessionManager.IsLoggedIn())
                {
                    var user = SessionManager.GetInstance.User;
                    idiomaDAL.SetUserIdioma(user.Id, idioma.Id);
                    user.IdIdioma = idioma.Id;

                    bitacoraBLL.RegistrarEvento(NameEvent.CambiarIdioma,
                        $"Cambio de idioma a '{idioma.Codigo}'", Priority.Low);
                }

                Activar(idioma);
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                RegistrarFalloSistema("Falló el cambio de idioma", ex);
                return OperationResult.Fail("msg.generic.error");
            }
        }

        /// <summary>
        /// Al iniciar sesión: aplica el idioma guardado del usuario. Si nunca eligió uno (o el suyo
        /// ya no está activo) se conserva el que estaba en uso, que puede ser el elegido en el login.
        /// </summary>
        public void AplicarIdiomaDe(User user)
        {
            try
            {
                var guardado = user.IdIdioma.HasValue ? idiomaDAL.GetById(user.IdIdioma.Value) : null;

                if (guardado is { Activo: true })
                    Activar(guardado);
                else if (activo == null)
                    Activar(idiomaDAL.GetDefault());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        // ---- Administración (requiere GESTIONAR_IDIOMAS) ----

        public OperationResult CrearIdioma(string codigo, string nombre)
        {
            if (!Authorize("crear un idioma")) return Denied();

            codigo = codigo.Trim().ToLowerInvariant();
            nombre = nombre.Trim();

            if (!CodigoValido.IsMatch(codigo)) return OperationResult.Fail("msg.idioma.codigoInvalido");
            if (nombre.Length == 0 || nombre.Length > MaxNombreLength)
                return OperationResult.Fail("msg.idioma.nombreInvalido", MaxNombreLength);

            try
            {
                if (idiomaDAL.GetByCodigo(codigo) != null)
                    return OperationResult.Fail("msg.idioma.yaExiste");
            }
            catch (Exception ex)
            {
                RegistrarFalloSistema("Falló la consulta de idiomas", ex);
                return OperationResult.Fail("msg.generic.error");
            }

            return Execute(NameEvent.CrearIdioma, $"Creación del idioma '{codigo}'",
                () => idiomaDAL.Insert(new Idioma { Codigo = codigo, Nombre = nombre }),
                "msg.idioma.creado");
        }

        public OperationResult ActualizarIdioma(Idioma idioma, string nombre, bool activoNuevo)
        {
            if (!Authorize("modificar un idioma")) return Denied();

            nombre = nombre.Trim();
            if (nombre.Length == 0 || nombre.Length > MaxNombreLength)
                return OperationResult.Fail("msg.idioma.nombreInvalido", MaxNombreLength);
            if (idioma.EsDefault && !activoNuevo)
                return OperationResult.Fail("msg.idioma.defaultNoSeDesactiva");

            return Execute(NameEvent.ModificarIdioma, $"Modificación del idioma '{idioma.Codigo}'", () =>
            {
                idioma.Nombre = nombre;
                idioma.Activo = activoNuevo;
                idiomaDAL.Update(idioma);
            }, "msg.idioma.actualizado");
        }

        public List<TraduccionItem> GetTraducciones(Idioma idioma) => idiomaDAL.GetTraducciones(idioma.Id);

        /// <summary>
        /// Guarda (o, con texto vacío, quita) la traducción de una etiqueta. Si afecta al idioma
        /// en uso o al de respaldo, se recarga y se avisa a los suscriptores.
        /// </summary>
        public OperationResult GuardarTraduccion(Idioma idioma, TraduccionItem item, string? texto)
        {
            if (!Authorize("actualizar una traducción")) return Denied();

            texto = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
            if (texto != null && texto.Length > MaxTextoLength)
                return OperationResult.Fail("msg.idioma.textoLargo", MaxTextoLength);

            return Execute(NameEvent.ActualizarTraduccion,
                $"Traducción '{item.Clave}' en '{idioma.Codigo}' {(texto == null ? "quitada" : "actualizada")}", () =>
                {
                    idiomaDAL.UpsertTraduccion(item.IdEtiqueta, idioma.Id, texto);
                    item.Texto = texto;

                    if (activo != null && (idioma.Id == activo.Id || idioma.EsDefault))
                        Activar(activo);
                }, "msg.idioma.traduccionGuardada");
        }

        // ---- Soporte ----

        /// <summary>Carga perezosa del idioma por defecto la primera vez que se pide un texto.</summary>
        private void Asegurar()
        {
            if (cargado) return;

            try
            {
                var idioma = idiomaDAL.GetDefault();
                if (idioma != null) Cargar(idioma);
            }
            catch (Exception ex)
            {
                // Sin base la UI muestra los textos de diseño; se reintenta en el próximo pedido.
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void Activar(Idioma? idioma)
        {
            if (idioma == null) return;

            Cargar(idioma);
            notifySuscribers();
        }

        /// <summary>Dos consultas por cambio: textos del idioma elegido y del default (respaldo).</summary>
        private void Cargar(Idioma idioma)
        {
            var defecto = idioma.EsDefault ? idioma : idiomaDAL.GetDefault();
            var deDefault = defecto == null ? new Dictionary<string, string>() : idiomaDAL.GetTextos(defecto.Id);
            var deActivo = idioma.EsDefault ? deDefault : idiomaDAL.GetTextos(idioma.Id);

            // Recién con las consultas hechas se pisa el estado: una falla no deja la caché a medias.
            porDefecto = defecto;
            textosDefault = deDefault;
            textosActivo = deActivo;
            activo = idioma;
            cargado = true;
        }

        private static Leyenda Armar(string clave, string texto, bool traducido, bool existe, object[] args)
        {
            if (args.Length > 0)
            {
                try { texto = string.Format(CultureInfo.CurrentCulture, texto, args); }
                catch (FormatException) { /* texto mal cargado desde la administración: se muestra tal cual */ }
            }

            return new Leyenda { Clave = clave, Texto = texto, Traducido = traducido, Existe = existe };
        }

        private bool Authorize(string action)
        {
            if (SessionManager.HasPermission(PermissionCode.GestionarIdiomas)) return true;

            bitacoraBLL.RegistrarError(NameEvent.AccesoNoAutorizado,
                $"Intento no autorizado de {action} (requiere {PermissionCode.GestionarIdiomas})", Priority.High);

            return false;
        }

        private static OperationResult Denied() => OperationResult.Fail("msg.noPermiso");

        private OperationResult Execute(NameEvent nameEvent, string detail, Action action, string okKey)
        {
            try
            {
                action();
                bitacoraBLL.RegistrarEvento(nameEvent, detail, Priority.Medium);
                return OperationResult.Ok(okKey);
            }
            catch (Exception ex)
            {
                RegistrarFalloSistema(detail, ex);
                return OperationResult.Fail("msg.generic.error");
            }
        }

        private void RegistrarFalloSistema(string detail, Exception ex)
        {
            if (SessionManager.IsLoggedIn())
                bitacoraBLL.RegistrarError(NameEvent.ErrorSistema, $"{detail}: {ex.Message}", Priority.High);
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
