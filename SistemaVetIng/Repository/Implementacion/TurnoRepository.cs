using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

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
    }
}
