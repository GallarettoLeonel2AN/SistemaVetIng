namespace SistemaVetIng.Models.Decorator
{
    public class RecargoFinDeSemana : AtencionDecorator
    {
        public RecargoFinDeSemana(ICostoAtencion atencionDecorada) : base(atencionDecorada)
        {
        }

        public override decimal Calcular()
        {
            var costo = base.Calcular();
            var dia = DateTime.Now.DayOfWeek;

            // Si es Sábado o Domingo
            if (dia == DayOfWeek.Saturday || dia == DayOfWeek.Sunday)
            {
                
                return costo * 1.10m;
            }

            return costo;
        }
    }
}
