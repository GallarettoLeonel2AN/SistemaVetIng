using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using X.PagedList;
using X.PagedList.EF;

namespace SistemaVetIng.Repository.Implementacion
{
    public class TurnoRepository : ITurnoRepository
    {
        private readonly ApplicationDbContext _context;

        public TurnoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Turno>> GetTurnosByFecha(DateTime fecha)
        {
            var fechaSinHora = fecha.Date;
            return await _context.Turnos
                .Where(t => t.Fecha.Date == fechaSinHora && t.Estado == "Pendiente")
                .ToListAsync();
        }

        public async Task AgregarTurno(Turno turno)
        {
             await _context.Turnos.AddAsync(turno);
        }

        public async Task Guardar()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Turno>> ListarTodo()
        {
            return await _context.Turnos
                                 .Include(t => t.Cliente)
                                 .Include(t => t.Mascota)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Turno>> ObtenerTurnosPorClienteIdAsync(int clienteId)
        {
            return await _context.Turnos
                                 .Include(t => t.Cliente)
                                 .Include(t => t.Mascota)
                                 .Where(t => t.ClienteId == clienteId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Turno>> ObtenerTurnosPorFechaAsync(DateTime fecha)
        {
            return await _context.Turnos
                                 .Include(t => t.Cliente)
                                 .Include(t => t.Mascota)
                                 .Where(t => t.Fecha.Date == fecha.Date) 
                                 .OrderBy(t => t.Horario) 
                                 .ToListAsync();
        }

        public async Task<Turno> ObtenerPorIdConDatosAsync(int id)
        {
            return await _context.Turnos
                                 .Include(t => t.Cliente) 
                                 .Include(t => t.Mascota)   
                                 .FirstOrDefaultAsync(t => t.Id == id); 
        }

        public void Actualizar(Turno turno)
        {
            _context.Turnos.Update(turno);
        }

        public async Task<IPagedList<Turno>> ListarPaginadoPorClienteAsync(int clienteId, int pageNumber, int pageSize, string busqueda = null)
        {
            var query = _context.Turnos
                                .Where(t => t.ClienteId == clienteId) 
                                .Include(t => t.Mascota) 
                                .Include(t => t.Cliente).AsQueryable(); 


            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(t => t.Mascota != null && t.Mascota.Nombre.Contains(busqueda));
            }

            query = query.OrderByDescending(t => t.Fecha).ThenBy(t => t.Horario);

            return await query.ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task<int> ContarTurnosParaFechaAsync(DateTime fecha)
        {
            return await _context.Turnos
                                 .CountAsync(t => t.Fecha.Date == fecha.Date);
        }
        public async Task<int> ContarTotalTurnosClienteAsync(int idCliente)
        {
            return await _context.Turnos
                                 .CountAsync(t => t.ClienteId == idCliente);
        }

        public async Task<int> CantidadTurnosPendientesPorCliente(int idCliente)
        {
            return await _context.Turnos
                .Where(c => c.ClienteId == idCliente && c.Estado == "Pendiente")
                .CountAsync();
        }
        public async Task<int> ContarTurnosPorEstadoAsync(string estado)
        {
            return await _context.Turnos.CountAsync(t => t.Estado == estado);
        }

        public async Task<int> CantidadTurnosAsync()
        {
            return await _context.Turnos.CountAsync();
        }

        public async Task<int> ContarTurnosPorEstadoYFechaAsync(string estado, DateTime fecha)
        {
            return await _context.Turnos
                             .CountAsync(t => t.Fecha.Date == fecha.Date && t.Estado == estado);
        }
    }
}
