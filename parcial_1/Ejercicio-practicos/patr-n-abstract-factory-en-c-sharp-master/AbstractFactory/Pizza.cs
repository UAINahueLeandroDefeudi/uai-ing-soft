
namespace AbstractFactory
{
    public abstract class Pizza
    {
        protected string _descripcion;

        public object Descripcion
        {
            get
            {
                return _descripcion;
            }
        }
    }
}