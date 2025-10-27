using SistemaVetIng.Models;
public interface IPagoRepository
{
    Task<Pago> CrearPagoAsync(Pago pago);
}