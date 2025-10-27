
using SistemaVetIng.Data;
using SistemaVetIng.Models;

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
}