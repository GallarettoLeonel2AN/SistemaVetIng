using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using static SistemaVetIng.Models.Extension.Permission;

namespace SistemaVetIng.Repository.Implementacion
{
    public class ChipRepository : IChipRepository
    {

        private readonly ApplicationDbContext _context;

        public ChipRepository(ApplicationDbContext contexto)
        {
            _context = contexto;
        }

        public async Task<IEnumerable<Chip>> ListarTodo()
         => await _context.Chips.ToListAsync();


        public async Task<Chip> ObtenerPorId(int id)
            => await _context.Chips.FirstOrDefaultAsync(c => c.Id == id);

        public async Task<bool> PoseeChipMascota(int mascotaId)
           =>  await _context.Chips.AnyAsync(c => c.MascotaId == mascotaId);


        public async Task Agregar(Chip entity)
            => await _context.Chips.AddAsync(entity);

        public void Modificar(Chip entity)
        {
            _context.Chips.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Eliminar(Chip entity)
            => _context.Chips.Remove(entity);

        public async Task Guardar()
            => await _context.SaveChangesAsync();

        public async Task<Chip> ObtenerPorMascotaId(int mascotaId)
            => await _context.Chips.FirstOrDefaultAsync(c => c.MascotaId == mascotaId);
    }
}
