using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Globalization;

namespace SistemaVetIng.Repository.Implementacion
{
    public class AtencionVeterinariaRepository : IAtencionVeterinariaRepository
    {
        private readonly ApplicationDbContext _context;

        public AtencionVeterinariaRepository(ApplicationDbContext context)
        {
            _context = context;
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
            
            return await _context.AtencionesVeterinarias
                .Include(a => a.HistoriaClinica) 
                .ThenInclude(hc => hc.Mascota)   
                .Where(a => a.HistoriaClinica.Mascota.ClienteId == clienteId)
                // Filtramos por el estado que indica que falta el pago
                .Where(a => a.Abonado == false)
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

        public async Task<int> CantidadAtencionesPorVeterinario(int id)
        {
            return await _context.AtencionesVeterinarias.Where(v => v.VeterinarioId == id).CountAsync();
        }
        public async Task<int> CantidadAtenciones()
        {
            return await _context.AtencionesVeterinarias.CountAsync();
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

        public async Task<Cliente> ObtenerClienteMasFrecuenteAsync()
        {

            var clienteIdMasFrecuente = await _context.AtencionesVeterinarias
                .Include(a => a.HistoriaClinica.Mascota)
                .Where(a => a.HistoriaClinica.Mascota != null && a.HistoriaClinica.Mascota.ClienteId != 0)
                .GroupBy(a => a.HistoriaClinica.Mascota.ClienteId) 
                .Select(g => new { ClienteId = g.Key, Count = g.Count() }) 
                .OrderByDescending(x => x.Count) 
                .Select(x => x.ClienteId)        
                .FirstOrDefaultAsync();        

            if (clienteIdMasFrecuente == 0)
            {
                return null; 
            }

            return await _context.Clientes
                                 .Include(c => c.Usuario) 
                                 .FirstOrDefaultAsync(c => c.Id == clienteIdMasFrecuente);
        }

        public async Task<decimal> SumarIngresosAsync()
        {
            return await _context.AtencionesVeterinarias.SumAsync(a => a.CostoTotal);
        }

        public async Task<List<DashboardViewModel.ServicioCountData>> ContarTopServiciosAsync(int topN)
        {
    
            // Contar Vacunas Aplicadas
            var topVacunas = await _context.AtencionesVeterinarias
                .SelectMany(a => a.Vacunas) // Expande las listas de vacunas
                .GroupBy(v => v.Nombre) // Agrupa por nombre de vacuna
                .Select(g => new DashboardViewModel.ServicioCountData
                {
                    NombreServicio = g.Key,
                    CantidadSolicitudes = g.Count(),
                    Tipo = "Vacuna" // Identifica el tipo
                })
                .OrderByDescending(x => x.CantidadSolicitudes)
                .Take(topN) // Limita a los N más altos
                .ToListAsync();

            // Contar Estudios Realizados
            var topEstudios = await _context.AtencionesVeterinarias
                .SelectMany(a => a.EstudiosComplementarios) // Expande las listas de estudios
                .GroupBy(e => e.Nombre) // Agrupa por nombre de estudio
                .Select(g => new DashboardViewModel.ServicioCountData
                {
                    NombreServicio = g.Key,
                    CantidadSolicitudes = g.Count(),
                    Tipo = "Estudio" // Identifica el tipo
                })
                .OrderByDescending(x => x.CantidadSolicitudes)
                .Take(topN)
                .ToListAsync();

            // Combinar ambas listas, reordenar y tomar el top N final
            var topServiciosCombinados = topVacunas
                .Concat(topEstudios) // Une las dos listas
                .OrderByDescending(x => x.CantidadSolicitudes) // Reordena combinados
                .Take(topN) // Asegura que solo devolvemos N en total
                .ToList();

            return topServiciosCombinados;
        }

        public async Task<List<DashboardViewModel.IngresosAnualesData>> ObtenerDatosIngresosAnualesAsync(List<int> anios)
        {
            if (anios == null || !anios.Any())
            {
                return new List<DashboardViewModel.IngresosAnualesData>(); // Devuelve lista vacía si no se piden años
            }

            var ingresosPorAnio = await _context.AtencionesVeterinarias
                .Where(a => anios.Contains(a.Fecha.Year)) 
                .GroupBy(a => a.Fecha.Year) 
                .Select(g => new // Proyecta a un objeto temporal
                {
                    Anio = g.Key, 
                    IngresoTotal = g.Sum(a => a.CostoTotal) 
                })
                .ToListAsync(); // Trae los resultados a memoria

            // Convierte los resultados al formato del ViewModel
            var resultadoViewModel = anios.Select(anio =>
            {
                var ingreso = ingresosPorAnio.FirstOrDefault(i => i.Anio == anio);
                return new DashboardViewModel.IngresosAnualesData
                {
                    Anio = anio.ToString(),
                    IngresoRealAnual = ingreso?.IngresoTotal ?? 0m, // Usa el ingreso calculado o 0 si no hubo
                };
            }).ToList();


            return resultadoViewModel;
        }

        public async Task<List<DashboardViewModel.IngresosMensualesData>> ObtenerDatosIngresosMensualesAsync(int anio)
        {
            var ingresosPorMes = await _context.AtencionesVeterinarias
            .Where(a => a.Fecha.Year == anio) 
            .GroupBy(a => a.Fecha.Month) 
            .Select(g => new
            {
                MesNumero = g.Key, // Número del mes (1-12)
                IngresoTotal = g.Sum(a => a.CostoTotal) // Suma ingresos del mes
            })
            .OrderBy(x => x.MesNumero) // Ordena por mes
            .ToListAsync();

            // Convertir a ViewModel, incluyendo nombre del mes y rellenando meses sin ingresos
            var cultureInfo = new CultureInfo("es-AR"); // Cultura para nombres de mes en español
            var resultadoViewModel = new List<DashboardViewModel.IngresosMensualesData>();

            for (int i = 1; i <= 12; i++)
            {
                var ingresoMes = ingresosPorMes.FirstOrDefault(m => m.MesNumero == i);
                resultadoViewModel.Add(new DashboardViewModel.IngresosMensualesData
                {
                    MesNumero = i,
                    // Obtiene nombre abreviado ("Ene", "Feb")
                    Mes = cultureInfo.DateTimeFormat.GetAbbreviatedMonthName(i).ToUpper().Replace(".", ""), 
                    IngresoRealMensual = ingresoMes?.IngresoTotal ?? 0m, 
                                                                        
                });
            }

            return resultadoViewModel;
        }

        public async Task<List<DashboardViewModel.AtencionesPorVeterinarioData>> ContarAtencionesPorVeterinarioAsync(DateTime? inicio, DateTime? fin)
        {
            var query = _context.AtencionesVeterinarias
                              .Include(a => a.Veterinario) 
                              .Where(a => a.VeterinarioId != null)
                              .AsQueryable();

            if (inicio.HasValue)
            {
                query = query.Where(a => a.Fecha.Date >= inicio.Value.Date);
            }

            if (fin.HasValue)
            {
                query = query.Where(a => a.Fecha.Date < fin.Value.Date.AddDays(1));
            }

            // Agrupar por VeterinarioId y Nombre/Apellido, luego contar y proyectar al DTO
            var resultado = await query
                .GroupBy(a => new { a.VeterinarioId, Nombre = a.Veterinario.Nombre, Apellido = a.Veterinario.Apellido }) 
                .Select(g => new DashboardViewModel.AtencionesPorVeterinarioData
                {
                    VeterinarioId = g.Key.VeterinarioId,
                    NombreVeterinario = $"{g.Key.Nombre} {g.Key.Apellido}", 
                    CantidadAtenciones = g.Count() // Cuenta las atenciones en cada grupo
                })
                .OrderByDescending(x => x.CantidadAtenciones) // Ordena por los que más atendieron
                .ToListAsync(); 

            return resultado;
        }

        public async Task<List<AtencionVeterinaria>> ObtenerAtencionesPorMesAsync(int anio, int mes)
        {
            return await _context.AtencionesVeterinarias
                                 .Where(a => a.Fecha.Year == anio && a.Fecha.Month == mes)
                                 .ToListAsync();
        }

        public async Task<List<AtencionVeterinaria>> ObtenerAtencionesPorIdCliente(List<int> ids)
        {
            return await _context.AtencionesVeterinarias
                .Include(a => a.HistoriaClinica.Mascota.Propietario.Usuario)
                .Where(a => ids.Contains(a.Id) && a.Abonado == false) 
                .ToListAsync();
        }

        public async Task ActualizarAtencionesAsync(List<AtencionVeterinaria> atenciones)
        {
            _context.AtencionesVeterinarias.UpdateRange(atenciones);
            await _context.SaveChangesAsync();
        }
        public async Task ActualizarAtencionAsync(AtencionVeterinaria atencion)
        {
            _context.AtencionesVeterinarias.Update(atencion);
            await _context.SaveChangesAsync();
        }
        public async Task<int> ContarAtencionesHistoricasPorCliente(int clienteId)
        {
            // Contamos cuántas atenciones tiene cualquier mascota de este dueño
            return await _context.AtencionesVeterinarias
                .Include(a => a.HistoriaClinica)
                .ThenInclude(hc => hc.Mascota)
                .Where(a => a.HistoriaClinica.Mascota.ClienteId == clienteId)
                .CountAsync();
        }
    }
}
