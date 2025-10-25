using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class AtencionVeterinariaRepository : IAtencionVeterinariaRepository
    {
        private readonly ApplicationDbContext _context;

        public AtencionVeterinariaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId)
        {
            return await _context.HistoriasClinicas
                                 .Include(hc => hc.Mascota)
                                     .ThenInclude(m => m.Propietario)
                                 .FirstOrDefaultAsync(hc => hc.Id == historiaClinicaId);
        }

        public async Task<AtencionVeterinaria> ObtenerPorId(int id)
            => await _context.AtencionesVeterinarias.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<AtencionVeterinaria> ObtenerAtencionConCliente(int idAtencion)
        {
            return await _context.AtencionesVeterinarias
                .Include(a => a.HistoriaClinica)
                    .ThenInclude(hc => hc.Mascota)
                        .ThenInclude(m => m.Propietario)
                        .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(a => a.Id == idAtencion);
        }
        public async Task<List<AtencionVeterinaria>> ObtenerAtencionesPendientesPorCliente(int clienteId)
        {
            const string ESTADO_PENDIENTE = "Pendiente";

            return await _context.AtencionesVeterinarias
                .Include(a => a.HistoriaClinica) 
                .ThenInclude(hc => hc.Mascota)   
                .Where(a => a.HistoriaClinica.Mascota.ClienteId == clienteId)
                // Filtramos por el estado que indica que falta el pago
                //.Where(a => a.EstadoDePago == ESTADO_PENDIENTE)
                .ToListAsync();
        }
        public async Task<List<Vacuna>> GetVacunas()
        {
            return await _context.Vacunas.ToListAsync();
        }

        public async Task<List<Estudio>> GetEstudios()
        {
            return await _context.Estudios.ToListAsync();
        }

        public async Task<Veterinario> GetVeterinarioPorId(int usuarioId)
        {
            return await _context.Veterinarios
                                 .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
        }

        public async Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids)
        {
            return await _context.Vacunas
                                 .Where(v => ids.Contains(v.Id))
                                 .ToListAsync();
        }

        public async Task<List<Estudio>> GetEstudioSeleccionado(IEnumerable<int> ids)
        {
            return await _context.Estudios
                                 .Where(e => ids.Contains(e.Id))
                                 .ToListAsync();
        }

        public async Task AgregarAtencionVeterinaria(AtencionVeterinaria atencion)
        {
            await _context.AtencionesVeterinarias.AddAsync(atencion);
        }

        public async Task AgregarTratamiento(Tratamiento tratamiento)
        {
            await _context.Tratamientos.AddAsync(tratamiento);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaPorId(int id)
        {
            return await _context.HistoriasClinicas.FindAsync(id);
        }

        public async Task<int> CantidadAtencionesPorVeterinario(int id)
        {
            return await _context.AtencionesVeterinarias.Where(v => v.VeterinarioId == id).CountAsync();
        }

        public async Task<decimal> SumarCostosAtencionesMesActualAsync()
        {
            var hoy = DateTime.Today;
            var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            return await _context.AtencionesVeterinarias
                                 .Where(a => a.Fecha >= primerDiaMes && a.Fecha <= ultimoDiaMes)
                                 .SumAsync(a => a.CostoTotal);
        }

        public async Task<(string Nombre, string Lote)> ObtenerVacunaMasFrecuenteAsync()
        {
            var vacunaInfo = await _context.AtencionesVeterinarias
                .SelectMany(a => a.Vacunas) 
                .GroupBy(v => new { v.Id, v.Nombre, v.Lote }) // agrupa 
                .Select(g => new { VacunaId = g.Key.Id, Nombre = g.Key.Nombre, Lote = g.Key.Lote, Count = g.Count() }) // Cuenta cuantas veces aparece cada una
                .OrderByDescending(x => x.Count) // ordena por mas frecuente
                .FirstOrDefaultAsync(); // tomamos la primera

            return vacunaInfo != null ? (vacunaInfo.Nombre, vacunaInfo.Lote) : (null, null);
        }

        public async Task<(string Nombre, decimal Precio)> ObtenerEstudioMasSolicitadoAsync()
        {
            var estudioInfo = await _context.AtencionesVeterinarias
                .SelectMany(a => a.EstudiosComplementarios)
                .GroupBy(e => new { e.Id, e.Nombre, e.Precio })
                .Select(g => new { EstudioId = g.Key.Id, Nombre = g.Key.Nombre, Precio = g.Key.Precio, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return estudioInfo != null ? (estudioInfo.Nombre, estudioInfo.Precio) : (null, 0m); // Devuelve 0 si no hay
        }

        public async Task<Mascota> ObtenerMascotaMasFrecuentePorVeterinario(int idVeterinario)
        {
            var atencionesDelVeterinario = _context.AtencionesVeterinarias
                .Where(a => a.VeterinarioId == idVeterinario);

            var mascotaIdMasFrecuente = await atencionesDelVeterinario
                .GroupBy(a => a.HistoriaClinica.MascotaId) 
                .Select(g => new { MascotaId = g.Key, Count = g.Count() }) 
                .OrderByDescending(x => x.Count) 
                .Select(x => x.MascotaId) 
                .FirstOrDefaultAsync(); 

            if (mascotaIdMasFrecuente == 0)
            {
                return null;
            }

            return await _context.Mascotas.FindAsync(mascotaIdMasFrecuente);
        }

        public async Task<int> CantidadPagosPendientes(int idCliente)
        {
            return await _context.AtencionesVeterinarias
                .Where(a => a.HistoriaClinica.Mascota.Propietario.Id == idCliente && a.Abonado == false)
                .CountAsync();
         
        }
    }
}
