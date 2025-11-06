using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Servicios.Interfaces;
using X.PagedList;
using X.PagedList.EF;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarEventoAsync(int usuarioId, string nombreUsuario, string tipoEvento, string entidad,string? detalles = null)
        {
            try
            {
                var log = new AuditoriaEvento
                {
                    UsuarioId = usuarioId,
                    NombreUsuario = nombreUsuario,
                    TipoEvento = tipoEvento,
                    Entidad = entidad,
                    FechaHora = DateTime.Now,
                    Detalles = detalles
                };

                await _context.AuditoriaEventos.AddAsync(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar auditoría: {ex.Message}");
            }
        }

        public async Task<IPagedList<AuditoriaEvento>> ObtenerLogPaginadoAsync(int pageNumber, int pageSize, string? busquedaUsuario = null, string? tipoEvento = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = _context.AuditoriaEventos
                                .Include(a => a.Usuario) 
                                .AsQueryable();

            // Aplicar filtro por Nombre de Usuario
            if (!string.IsNullOrWhiteSpace(busquedaUsuario))
            {
                string busquedaLower = busquedaUsuario.ToLower();
                query = query.Where(a => a.NombreUsuario.ToLower().Contains(busquedaLower));
            }

            // Aplicar filtro por Tipo de Evento
            if (!string.IsNullOrWhiteSpace(tipoEvento))
            {
                query = query.Where(a => a.TipoEvento == tipoEvento);
            }

            // Aplicar filtro por Fecha de Inicio
            if (fechaInicio.HasValue)
            {
                query = query.Where(a => a.FechaHora >= fechaInicio.Value);
            }

            // Aplicar filtro por Fecha de Fin
            if (fechaFin.HasValue)
            {
                var fechaFinReal = fechaFin.Value.AddDays(1);
                query = query.Where(a => a.FechaHora < fechaFinReal);
            }

            query = query.OrderByDescending(a => a.FechaHora);

            return await query.ToPagedListAsync(pageNumber, pageSize);
        }
    }
}
