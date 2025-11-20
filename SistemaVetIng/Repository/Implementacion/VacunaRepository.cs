using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class VacunaRepository : IVacunaRepository
    {
        private readonly ApplicationDbContext _context;

        public VacunaRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task Agregar(Vacuna entity) => await _context.Vacunas.AddAsync(entity);

        public async Task Guardar() => await _context.SaveChangesAsync();

        public async Task<IEnumerable<Vacuna>> ListarTodo() =>
            await _context.Vacunas.OrderBy(v => v.Nombre).ThenBy(v => v.Lote).ToListAsync();

        public void Modificar(Vacuna entity)
        {
            _context.Vacunas.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task<Vacuna> ObtenerPorId(int id) =>
            await _context.Vacunas.FirstOrDefaultAsync(v => v.Id == id);

        public void Eliminar(Vacuna entity) => _context.Vacunas.Remove(entity);


        public async Task<Vacuna> ObtenerPorIdAsync(int id)
        {
            return await _context.Vacunas.FindAsync(id);
        }

        public async Task<IEnumerable<Vacuna>> ObtenerPorIdsAsync(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new List<Vacuna>();
            }

            return await _context.Vacunas.Where(v => ids.Contains(v.Id)).ToListAsync();
        }

        public async Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids)
        {
            return await _context.Vacunas
                                 .Where(v => ids.Contains(v.Id))
                                 .ToListAsync();
        }
    }
}
