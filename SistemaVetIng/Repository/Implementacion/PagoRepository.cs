
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using System.Drawing.Printing;
using X.PagedList;
using X.PagedList.EF;

public class PagoRepository : IPagoRepository
{
    private readonly ApplicationDbContext _context;

    public PagoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Pago> CrearPagoAsync(Pago pago)
    {
        await _context.Pagos.AddAsync(pago);

        await _context.SaveChangesAsync();

        return pago;
    }

    public async Task<IPagedList<Pago>> ListarHistorialPagos(int clienteId, int pageNumber, int pageSize)
    {

        var query = _context.Pagos
                          .Where(p => p.ClienteId == clienteId);

        query = query.OrderByDescending(p => p.Fecha);

        return await query.ToPagedListAsync(pageNumber, pageSize);
    }
}