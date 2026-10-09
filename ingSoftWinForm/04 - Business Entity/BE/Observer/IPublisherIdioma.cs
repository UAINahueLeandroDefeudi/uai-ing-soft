namespace BE.Observer
{
    /// <summary>Subject del Observer de idiomas (T05). Misma forma que IPublisherMateria del ejemplo.</summary>
    public interface IPublisherIdioma
    {
        IReadOnlyList<ISuscriberIdioma> suscribers { get; }

        void suscribe(ISuscriberIdioma suscriber);

        void unsuscribe(ISuscriberIdioma suscriber);

        void notifySuscribers();
    }
}
