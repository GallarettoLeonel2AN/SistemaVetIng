
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using X.PagedList;

public class PagoService : IPagoService
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IAtencionVeterinariaRepository _atencionVeterinariaRepository;

    
    public PagoService(IPagoRepository pagoRepository, IAtencionVeterinariaRepository atencionVeterinariaRepository)
    {
        _pagoRepository = pagoRepository;
        _atencionVeterinariaRepository = atencionVeterinariaRepository;
    }

    public async Task<Pago> CrearPagoAsync(Pago pago)
    {
       
        return await _pagoRepository.CrearPagoAsync(pago);
    }
    public async Task<IPagedList<Pago>> ListarHistorialPagos(int clienteId, int pageNumber, int pageSize)
    {
        return await _pagoRepository.ListarHistorialPagos(clienteId,pageNumber,pageSize);
    }

    public async Task<bool> CrearPagoPresencialAsync(int atencionId, int clienteId, decimal monto, int metodoPagoId)
    {
        try
        {
            var atencion = await _atencionVeterinariaRepository.ObtenerPorId(atencionId);
            if (atencion == null)
            {
                return false;
            }

            var nuevoPago = new Pago
            {
                ClienteId = clienteId,
                Fecha = DateTime.Now,
                MontoTotal = monto,
                MetodoPagoId = metodoPagoId,
                Estado = "Pagado",
            };

            await _pagoRepository.CrearPagoAsync(nuevoPago);

            atencion.Abonado = true;
            atencion.PagoId = nuevoPago.Id;
            
            await _atencionVeterinariaRepository.ActualizarAtencionAsync(atencion);

            return true;
        }
        catch (Exception ex) { return false; }
            
    }
}