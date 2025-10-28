
using SistemaVetIng.Models;
using X.PagedList;

public interface IPagoService
{
    Task<Pago> CrearPagoAsync(Pago pago);
    Task<IPagedList<Pago>> ListarHistorialPagos(int clienteId, int pageNumber, int pageSize);
    Task<bool> CrearPagoPresencialAsync(int atencionId, int clienteId, decimal monto, int metodoPagoId);
}