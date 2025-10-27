
using SistemaVetIng.Models;

public class PagoService : IPagoService
{
    private readonly IPagoRepository _pagoRepository;

    
    public PagoService(IPagoRepository pagoRepository)
    {
        _pagoRepository = pagoRepository;
    }

    public async Task<Pago> CrearPagoAsync(Pago pago)
    {
       
        return await _pagoRepository.CrearPagoAsync(pago);
    }
}