namespace SistemaVetIng.Models.Decorator
{
    public class DescuentoClienteFrecuente : AtencionDecorator
    {
        public DescuentoClienteFrecuente(ICostoAtencion atencionDecorada) : base(atencionDecorada)
        {
        }

        public override decimal Calcular()
        {
            var costoAcumulado = base.Calcular();

            
            return costoAcumulado * 0.90m;
        }
    }
}


