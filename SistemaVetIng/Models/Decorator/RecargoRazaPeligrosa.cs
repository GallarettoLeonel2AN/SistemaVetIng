namespace SistemaVetIng.Models.Decorator
{
    public class RecargoRazaPeligrosa : AtencionDecorator
    {
        public RecargoRazaPeligrosa(ICostoAtencion atencionDecorada) : base(atencionDecorada)
        {
        }

        public override decimal Calcular()
        {

            
            return base.Calcular() * 1.05m;
        }
    }
}
