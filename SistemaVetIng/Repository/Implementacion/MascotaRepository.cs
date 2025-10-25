using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using X.PagedList;
using X.PagedList.EF;

namespace SistemaVetIng.Repository.Implementacion
{
    public class MascotaRepository : IMascotaRepository
    {
        private readonly ApplicationDbContext _context;

        public MascotaRepository(ApplicationDbContext contexto)
        {
            _context = contexto;
        }
        
        public async Task Agregar(Mascota entity)
            => await _context.Mascotas.AddAsync(entity);

       public async Task Guardar()
            => await _context.SaveChangesAsync();

        public async Task<IEnumerable<Mascota>> ListarTodo()
            =>  await _context.Mascotas.Include(m => m.Propietario).ToListAsync();

        public void Modificar(Mascota entity)
        {
            _context.Mascotas.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task<Mascota> ObtenerPorId(int id)
            =>  await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == id);

        public void Eliminar(Mascota entity)
            => _context.Mascotas.Remove(entity);

        public async Task<Mascota> ObtenerMascotaChipPorId(int id)
        {
            return await _context.Mascotas.Include(m => m.Chip).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Mascota>> ListarMascotasPorClienteId(int clienteId)
        {
            return await _context.Mascotas
                .Where(m => m.ClienteId == clienteId)
                .Include(m => m.Propietario)
                .ToListAsync();
        }

        public async Task<IPagedList<Mascota>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null)
        {
            var query = _context.Mascotas.Include(m => m.Propietario).AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(m => m.Nombre.Contains(busqueda) ||
                                         m.Especie.Contains(busqueda) ||
                                         m.Raza.Contains(busqueda) ||
                                         (m.Propietario != null && (m.Propietario.Nombre.Contains(busqueda) || m.Propietario.Apellido.Contains(busqueda))));
            }

            query = query.OrderBy(m => m.Nombre);

            return await query.ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task<int> ContarTotalMascotasAsync()
        {
            return await _context.Mascotas.CountAsync();
        }
        public async Task<int> ContarTotalMascotasPorClienteAsync(int idCliente)
        {
            return await _context.Mascotas
                .Where(m => m.ClienteId == idCliente)
                .CountAsync();
        }
        public async Task<int> ContarPerrosPeligrososAsync()
        {
            return await _context.Mascotas.CountAsync(m => m.RazaPeligrosa);
        } 
    }
}
