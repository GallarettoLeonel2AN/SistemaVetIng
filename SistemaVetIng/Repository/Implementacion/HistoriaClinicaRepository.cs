using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class HistoriaClinicaRepository : IHistoriaClinicaRepository
    {
        private readonly ApplicationDbContext _context;

        public HistoriaClinicaRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<Mascota> GetHistoriaClinicaCompletaMascota(int mascotaId)
        {
            return await _context.Mascotas
                                 .Include(m => m.Propietario)
                                 .Include(m => m.HistoriaClinica)
                                     .ThenInclude(hc => hc.Atenciones)
                                         .ThenInclude(a => a.Tratamiento)
                                 .Include(m => m.HistoriaClinica)
                                     .ThenInclude(hc => hc.Atenciones)
                                         .ThenInclude(a => a.Veterinario)
                                 .Include(m => m.HistoriaClinica)
                                     .ThenInclude(hc => hc.Atenciones)
                                         .ThenInclude(a => a.Vacunas)
                                 .Include(m => m.HistoriaClinica)
                                     .ThenInclude(hc => hc.Atenciones)
                                         .ThenInclude(a => a.EstudiosComplementarios)
                                 .FirstOrDefaultAsync(m => m.Id == mascotaId);
        }

        public async Task<HistoriaClinica> ObtenerPorMascotaIdAsync(int mascotaId)
        {
            return await _context.HistoriasClinicas.FirstOrDefaultAsync(h => h.MascotaId == mascotaId);
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId)
        {
            return await _context.HistoriasClinicas
                                 .Include(hc => hc.Mascota)
                                     .ThenInclude(m => m.Propietario)
                                 .FirstOrDefaultAsync(hc => hc.Id == historiaClinicaId);
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaPorId(int id)
        {
            return await _context.HistoriasClinicas.FindAsync(id);
        }
    }
}

