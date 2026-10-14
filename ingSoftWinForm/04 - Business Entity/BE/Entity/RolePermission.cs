namespace BE.Entity
{
    /// <summary>
    /// Rol (Modelo I): un compuesto que se asigna a usuarios. Puede contener permisos
    /// simples, compuestos u otros roles (rol anidado).
    /// </summary>
    public class RolePermission : CompoundPermission
    {
        public override bool IsRol => true;
    }
}
