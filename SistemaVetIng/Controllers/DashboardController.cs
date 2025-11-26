using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Models;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Globalization;
using static SistemaVetIng.ViewsModels.DashboardViewModel;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinaria,Veterinario")]
    public class DashboardController : Controller
    {

        private readonly ITurnoService _turnoService;
        private readonly IAtencionVeterinariaService _atencionVeterinariaService;
        private readonly IClienteService _clienteService;
        private readonly IMascotaService _mascotaService;


        public DashboardController(
            ITurnoService turnoService,
            IAtencionVeterinariaService atencionVeterinariaService,
            IClienteService clienteService,
            IMascotaService mascotaService)
        {
            _turnoService = turnoService;
            _atencionVeterinariaService = atencionVeterinariaService;
            _clienteService = clienteService;
            _mascotaService = mascotaService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new DashboardViewModel();
            var hoy = DateTime.Today;

            // Llamadas a Servicios
            viewModel.PorcentajeAusencia = await _turnoService.CalcularPorcentajeAusentismoAsync();
            viewModel.EstadoSemaforoAusencia = CalcularEstadoSemaforoAusencia(viewModel.PorcentajeAusencia);

            viewModel.TotalAtencionesRealizadas = await _atencionVeterinariaService.CantidadAtenciones();

            viewModel.TotalCantidadDeTurnos = await _turnoService.CantidadTurnosAsync();
            viewModel.ClienteMasFrecuente = await _atencionVeterinariaService.ObtenerClienteMasFrecuenteAsync();
            viewModel.TotalMascotas = await _mascotaService.ContarTotalMascotasAsync();
            viewModel.PerrosPeligrosos = await _mascotaService.ContarPerrosPeligrososAsync();
            viewModel.MascotasConChipCount = await _mascotaService.ContarPerrosChipAsync();
            viewModel.TotalIngresosHistoricos = await _atencionVeterinariaService.SumarIngresosAsync();
            viewModel.IngresoPromedioPorAtencion = (viewModel.TotalAtencionesRealizadas > 0) ? viewModel.TotalIngresosHistoricos / viewModel.TotalAtencionesRealizadas : 0;
            viewModel.TotalClientes = await _clienteService.ContarTotalClientesAsync();


            // GRAFICO 1: Atenciones por Veterinario 
            viewModel.AtencionesPorVeterinario = await _atencionVeterinariaService.ContarAtencionesPorVeterinarioAsync(null, null);

            // GRAFICO 2: Distribución por Especie
            viewModel.DistribucionEspecies = await _mascotaService.ContarMascotasPorEspecieAsync();

            foreach (var especie in viewModel.DistribucionEspecies)
            {
                try
                {
                    // Obtener razas para esa especie y mapear a RazaData
                    var razas = await _mascotaService.ObtenerRazasPorEspecieAsync(especie.Especie);
                    especie.Razas = razas.Select(r => new RazaData { Nombre = r.Nombre, Cantidad = r.Cantidad }).ToList();
                }
                catch
                {
                    especie.Razas = new List<RazaData>();
                }
            }

            // GRAFICO 3: Servicios MAS Solicitados (Top 5)
            viewModel.TopServicios = await _atencionVeterinariaService.ContarTopServiciosAsync(5);

            // GRAFICO 4: Ingresos Anuales/Mensuales/Semanales
            var aniosAConsultar = new List<int> { hoy.Year - 2, hoy.Year - 1, hoy.Year };
            viewModel.IngresosAnuales = await _atencionVeterinariaService.ObtenerDatosIngresosAnualesAsync(aniosAConsultar);

            try
            {
                var allTurnos = await _turnoService.ObtenerTurnosAsync();
                viewModel.TotalTurnosPendientes = allTurnos.Count(t => t.Estado == "Pendiente");
                viewModel.TotalTurnosFinalizados = allTurnos.Count(t => t.Estado == "Finalizado");
                viewModel.TotalTurnosCancelados = allTurnos.Count(t => t.Estado == "Cancelado");
                viewModel.TotalTurnosNoAsistio = allTurnos.Count(t => t.Estado == "No Asistió");
            }
            catch
            {
                viewModel.TotalTurnosPendientes = 0;
                viewModel.TotalTurnosFinalizados = 0;
                viewModel.TotalTurnosCancelados = 0;
                viewModel.TotalTurnosNoAsistio = 0;
            }


            foreach (var anioData in viewModel.IngresosAnuales)
            {
                int anioActual = int.Parse(anioData.Anio);
                anioData.MetaAnual = ObtenerMetaAnual(anioActual);
                anioData.EstadoSemaforo = CalcularEstadoSemaforo(anioData.IngresoRealAnual, anioData.MetaAnual);

                anioData.IngresosMensuales = await _atencionVeterinariaService.ObtenerDatosIngresosMensualesAsync(anioActual);
                foreach (var mesData in anioData.IngresosMensuales)
                {
                    mesData.MetaMensual = ObtenerMetaMensual(anioActual, mesData.MesNumero);
                    mesData.EstadoSemaforo = CalcularEstadoSemaforo(mesData.IngresoRealMensual, mesData.MetaMensual);

                    var atencionesDelMes = await _atencionVeterinariaService.ObtenerAtencionesPorMesAsync(anioActual, mesData.MesNumero);

                    mesData.IngresosSemanales = CalcularIngresosSemanales(
                        atencionesDelMes,
                        anioActual,
                        mesData.MesNumero);
                }
            }

            return View(viewModel);
        }

        // --- METODOS Helpers ---
        private string CalcularEstadoSemaforo(decimal real, decimal meta)
        {
            decimal cumplimiento = (meta > 0) ? real / meta : (real > 0 ? 1 : 0);
            string estado = "rojo";
            if (cumplimiento >= 0.95m) { estado = "verde"; }
            else if (cumplimiento >= 0.85m) { estado = "amarillo"; }
            return estado;
        }

        private string CalcularEstadoSemaforoAusencia(double porcentajeAusencia)
        {
            if (porcentajeAusencia <= 10.0) return "verde";
            if (porcentajeAusencia <= 20.0) return "amarillo";
            return "rojo";
        }

        // --- Metodos para Metas  ---
        private decimal ObtenerMetaAnual(int anio)
        {
            if (anio == DateTime.Today.Year) return 650000m; 
            return 300000m; 
        }
        private decimal ObtenerMetaMensual(int anio, int mes)
        {
            return ObtenerMetaAnual(anio) / 12; 
        }

        // --- Helper para Calcular Ingresos Semanales ---
        private List<DashboardViewModel.IngresosSemanalesData> CalcularIngresosSemanales(List<AtencionVeterinaria> atencionesDelMes, int anio, int mes)
        {
            var ingresosSemanales = new List<DashboardViewModel.IngresosSemanalesData>();
            Calendar cal = CultureInfo.CurrentCulture.Calendar;
            DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

            var ingresosPorSemana = atencionesDelMes
                .GroupBy(a => GetWeekOfMonth(a.Fecha, firstDayOfWeek))
                .Select(g => new
                {
                    SemanaNum = g.Key,
                    IngresoSemanal = g.Sum(a => a.CostoTotal)
                })
                .OrderBy(x => x.SemanaNum);

            foreach (var semana in ingresosPorSemana)
            {
                ingresosSemanales.Add(new DashboardViewModel.IngresosSemanalesData
                {
                    Semana = $"Semana {semana.SemanaNum}",
                    IngresoRealSemanal = semana.IngresoSemanal
                });
            }

            int totalWeeks = GetTotalWeeksInMonth(anio, mes, firstDayOfWeek);
            for (int i = 1; i <= totalWeeks; i++)
            {
                if (!ingresosSemanales.Any(s => s.Semana == $"Semana {i}"))
                {
                    ingresosSemanales.Add(new DashboardViewModel.IngresosSemanalesData { Semana = $"Semana {i}", IngresoRealSemanal = 0m });
                }
            }

            return ingresosSemanales.OrderBy(s => int.Parse(s.Semana.Replace("Semana ", ""))).ToList();
        }

        private int GetWeekOfMonth(DateTime date, DayOfWeek firstDayOfWeek)
        {
            DateTime firstOfMonth = new DateTime(date.Year, date.Month, 1);
            int dayOfWeekFirst = (int)firstOfMonth.DayOfWeek;
            int offset = (dayOfWeekFirst - (int)firstDayOfWeek + 7) % 7;
            return (date.Day + offset - 1) / 7 + 1;
        }
        private int GetTotalWeeksInMonth(int year, int month, DayOfWeek firstDayOfWeek)
        {
            DateTime lastOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return GetWeekOfMonth(lastOfMonth, firstDayOfWeek);
        }

    }
}