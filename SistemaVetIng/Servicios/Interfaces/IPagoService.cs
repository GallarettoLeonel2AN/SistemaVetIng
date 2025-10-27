
using SistemaVetIng.Models;

public interface IPagoService
{
    
    Task<Pago> CrearPagoAsync(Pago pago);
}