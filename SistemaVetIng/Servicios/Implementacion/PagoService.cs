
using SistemaVetIng.Models;
using X.PagedList;

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
    public async Task<IPagedList<Pago>> ListarHistorialPagos(int clienteId, int pageNumber, int pageSize)
    {
        return await _pagoRepository.ListarHistorialPagos(clienteId,pageNumber,pageSize);
    }
}