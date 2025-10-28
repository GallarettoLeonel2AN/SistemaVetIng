namespace SistemaVetIng.ViewsModels
{
    public class PagosItemViewModel
    {
        public DateTime Fecha { get; set; }
        public string MetodoDePago { get; set; }
        public decimal Monto { get; set; }
        public string IconoCssClass { get; set; }
    }
}
