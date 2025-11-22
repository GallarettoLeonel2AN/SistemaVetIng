namespace SistemaVetIng.Models.Decorator
{
    public class CostoBaseAtencion : ICostoAtencion
    {
        private readonly decimal _consulta;
        private readonly decimal _vacunas;
        private readonly decimal _estudios;

        public CostoBaseAtencion(decimal consulta, decimal vacunas, decimal estudios)
        {
            _consulta = consulta;
            _vacunas = vacunas;
            _estudios = estudios;
        }

        public decimal Calcular()
        {
            return _consulta + _vacunas + _estudios;
        }
    }
}
