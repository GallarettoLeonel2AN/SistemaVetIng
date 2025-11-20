using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class EstudioRepository : IEstudioRepository
    {
        private readonly ApplicationDbContext _context;

        public EstudioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Agregar(Estudio entity) => await _context.Estudios.AddAsync(entity);
        public async Task Guardar() => await _context.SaveChangesAsync();

        public async Task<IEnumerable<Estudio>> ListarTodo() =>
            await _context.Estudios.OrderBy(e => e.Nombre).ToListAsync();

        public void Modificar(Estudio entity)
        {
            _context.Estudios.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task<Estudio> ObtenerPorId(int id) =>
            await _context.Estudios.FirstOrDefaultAsync(e => e.Id == id);

        public void Eliminar(Estudio entity) => _context.Estudios.Remove(entity);


        public async Task<Estudio> ObtenerPorIdAsync(int id)
        {
            return await _context.Estudios.FindAsync(id);
        }

        public async Task<IEnumerable<Estudio>> ObtenerPorIdsAsync(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new List<Estudio>();
            }

            return await _context.Estudios.Where(e => ids.Contains(e.Id)).ToListAsync();
        }

        public async Task<List<Estudio>> GetEstudioSeleccionado(IEnumerable<int> ids)
        {
            return await _context.Estudios
                                 .Where(e => ids.Contains(e.Id))
                                 .ToListAsync();
        }
    }
}
