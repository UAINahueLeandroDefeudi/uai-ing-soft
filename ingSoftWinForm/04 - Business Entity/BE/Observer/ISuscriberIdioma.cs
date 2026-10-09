using BE.Entity;

namespace BE.Observer
{
    /// <summary>
    /// Observer (T05): quien quiere enterarse del cambio de idioma. Recibe el idioma
    /// nuevo y no el publisher concreto, así no queda acoplado a la BLL.
    /// </summary>
    public interface ISuscriberIdioma
    {
        void Actualizar(Idioma idiomaActivo);
    }
}
