namespace BE.Base
{
    public abstract class BaseGuidEntity : BaseAuditEntity
    {
        public BaseGuidEntity()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
    }
}
