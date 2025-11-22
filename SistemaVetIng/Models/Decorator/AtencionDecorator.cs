namespace SistemaVetIng.Models.Decorator
{
    public abstract class AtencionDecorator : ICostoAtencion
    {
        protected readonly ICostoAtencion _atencionDecorada;

        public AtencionDecorator(ICostoAtencion atencionDecorada)
        {
            _atencionDecorada = atencionDecorada;
        }

        public virtual decimal Calcular()
        {
            return _atencionDecorada.Calcular();
        }
    }
}
