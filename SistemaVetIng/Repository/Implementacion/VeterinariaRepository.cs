using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class VeterinariaRepository : IVeterinariaRepository
    {
        private readonly ApplicationDbContext _context;

        public VeterinariaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Veterinaria> ObtenerPrimeraAsync()
        {
            return await _context.Veterinarias
                .Include(v => v.Veterinarios)
                .Include(v => v.Clientes)
                .Include(v => v.ConfiguracionVeterinaria)
                    .ThenInclude(c => c.HorariosPorDia)
                .FirstOrDefaultAsync();
        }

        public async Task Guardar()
        {
            await _context.SaveChangesAsync();
        }
    }
}
