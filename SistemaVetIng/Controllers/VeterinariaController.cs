using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Models;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinaria")]
    public class VeterinariaController : Controller
    {

        private readonly IToastNotification _toastNotification;
        private readonly IVeterinariaConfigService _veterinariaConfigService;
        private readonly IVeterinarioService _veterinarioService;
        private readonly IClienteService _clienteService;
        private readonly IMascotaService _mascotaService;
        private readonly ITurnoService _turnoService;
        

        public VeterinariaController(
            IVeterinariaConfigService service,
            IToastNotification toastNotification,
            IMascotaService mascotaService,
            IClienteService clienteService,
            IVeterinarioService veterinarioService,
            ITurnoService turnoService
            )
        {
            _mascotaService = mascotaService;
            _veterinarioService = veterinarioService;
            _clienteService = clienteService;
            _veterinariaConfigService = service;
            _toastNotification = toastNotification;
            _turnoService = turnoService;
           
        }

        #region PAGINA PRINCIPAL
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal(
            string busquedaVeterinario = null,
            string busquedaCliente = null,
            string busquedaMascota = null,
            int page = 1)
        {
            var viewModel = new VeterinariaPaginaPrincipalViewModel();
            int pageSizeClientes = 2;

            //  Cargar ConfiguracionHoraria

            var configuracionDb = await _veterinariaConfigService.ObtenerConfiguracionAsync();

            if (configuracionDb != null)
            {

                viewModel.ConfiguracionTurnos = new ConfiguracionVeterinariaViewModel
                {
                    Id = configuracionDb.Id,
                    DuracionMinutosPorConsulta = configuracionDb.DuracionMinutosPorConsulta,

                    Horarios = configuracionDb.HorariosPorDia.Select(h => new HorarioDiaViewModel
                    {
                        DiaSemana = h.DiaSemana,
                        EstaActivo = h.EstaActivo,
                        HoraInicio = (DateTime)h.HoraInicio,
                        HoraFin = (DateTime)h.HoraFin
                    }).OrderBy(h => h.DiaSemana).ToList() 
                };
            }
            else
            {
                viewModel.ConfiguracionTurnos = null;
            }


            //  Cargar Veterinario
            var veterinarios = string.IsNullOrWhiteSpace(busquedaVeterinario)
                ? await _veterinarioService.ListarTodo()
                : await _veterinarioService.FiltrarPorBusqueda(busquedaVeterinario);
            viewModel.Veterinarios = veterinarios.Select(p => new VeterinarioViewModel
            {
                Id = p.Id,
                NombreCompleto = $"{p.Nombre} {p.Apellido}",
                Telefono = p.Telefono,
                NombreUsuario = p.Usuario?.Email,
                Direccion = p.Direccion,
                Matricula = p.Matricula,
            }).ToList();


            // Cargar Clientes en las tablas

            var clientesPaginados = await _clienteService.ListarPaginadoAsync(page, pageSizeClientes, busquedaCliente);

            viewModel.Clientes = clientesPaginados.Select(c => new ClienteViewModel
            {
                Id = c.Id,
                NombreCompleto = $"{c.Nombre} {c.Apellido}",
                Telefono = c.Telefono,
                NombreUsuario = c.Usuario?.Email,
                DNI = c.Dni
            }).ToList();

            viewModel.PaginacionClientes = clientesPaginados;


            // Cargar Mascotas en las tablas
            var mascotas = string.IsNullOrWhiteSpace(busquedaMascota)
                ? await _mascotaService.ListarTodo()
                : await _mascotaService.FiltrarPorBusqueda(busquedaMascota);
            viewModel.Mascotas = mascotas.Select(m => new MascotaListViewModel
            {
                Id = m.Id,
                NombreMascota = m.Nombre,
                Especie = m.Especie,
                Sexo = m.Sexo,
                Raza = m.Raza,
                EdadAnios = DateTime.Today.Year - m.FechaNacimiento.Year - (DateTime.Today.Month < m.FechaNacimiento.Month || (DateTime.Today.Month == m.FechaNacimiento.Month && DateTime.Today.Day < m.FechaNacimiento.Day) ? 1 : 0),
                NombreDueno = $"{m.Propietario?.Nombre} {m.Propietario?.Apellido}",
                ClienteId = m.Propietario?.Id ?? 0
            }).ToList();


            // Cargar Listado de Turnos para la fecha actual
            var turnosDeHoy = await _turnoService.ObtenerTurnosPorFechaAsync(DateTime.Today);

            viewModel.CitasDeHoy = turnosDeHoy.Select(t => new TurnoViewModel
            {
                Id = t.Id,
                Horario = t.Horario,
                Motivo = t.Motivo,
                Estado = t.Estado,
                PrimeraCita = t.PrimeraCita,
                NombreMascota = t.Mascota?.Nombre,
                NombreCliente = $"{t.Cliente?.Nombre} {t.Cliente?.Apellido}"
            }).ToList();



            // Hardcoded de datos para Reportes Analíticos 
            viewModel.CantidadPerrosPeligrosos = 5;

            var razaMayorDemanda = mascotas
                 .GroupBy(m => m.Raza)
                 .OrderByDescending(g => g.Count())
                 .Select(g => g.Key)
                 .FirstOrDefault();

            viewModel.RazaMayorDemanda = razaMayorDemanda;
            viewModel.IngresosMensualesEstimados = 1500.00m;
            viewModel.IngresosDiariosEstimados = 5000.00m;

            return View(viewModel);
        }
        #endregion

        #region CONFIGURACION HORARIOS
        [HttpGet]
        public async Task<IActionResult> GuardarConfiguracion()
        {
            var config = await _veterinariaConfigService.ObtenerConfiguracionAsync();
            var viewModel = new ConfiguracionVeterinariaViewModel();

            if (config != null)
            {
                // Si ya existe una configuración, la mapeamos
                viewModel.Id = config.Id;
                viewModel.DuracionMinutosPorConsulta = config.DuracionMinutosPorConsulta;
                viewModel.Horarios = config.HorariosPorDia.Select(h => new HorarioDiaViewModel
                {
                    DiaSemana = h.DiaSemana,
                    EstaActivo = h.EstaActivo,
                    HoraInicio = h.HoraInicio.HasValue ? h.HoraInicio.Value : DateTime.MinValue,
                    HoraFin = h.HoraFin.HasValue ? h.HoraFin.Value : DateTime.MinValue
                }).OrderBy(h => h.DiaSemana).ToList();
            }
            else
            {
                // Si NO existe configuración, creamos una por defecto para la vista.
                viewModel.DuracionMinutosPorConsulta = 30; // Un valor inicial

                // Creamos una lista con los días de la semana (en orden)
                var diasDeLaSemana = new[] {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

                foreach (var dia in diasDeLaSemana)
                {
                    // Agregamos un objeto HorarioDiaViewModel por cada día a la lista
                    viewModel.Horarios.Add(new HorarioDiaViewModel
                    {
                        DiaSemana = dia,
                        EstaActivo = false // Empiezan desactivados
                    });
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracion(ConfiguracionVeterinariaViewModel model)
        {
            if (!ModelState.IsValid)
            {
               
                return View(model);
            }

            try
            {
                // Mapeamos el ViewModel al modelo de dominio.
                var configParaGuardar = new ConfiguracionVeterinaria
                {
                    Id = model.Id,
                    DuracionMinutosPorConsulta = model.DuracionMinutosPorConsulta,
                    HorariosPorDia = model.Horarios.Select(h => new HorarioDia
                    {
                        DiaSemana = h.DiaSemana,
                        EstaActivo = h.EstaActivo,
                        // Si está activo, usa el valor, si no, usa el valor por defecto.
                        HoraInicio = h.EstaActivo ? h.HoraInicio : DateTime.MinValue,
                        HoraFin = h.EstaActivo ? h.HoraFin : DateTime.MinValue
                    }).ToList()
                };

                await _veterinariaConfigService.Guardar(configParaGuardar);

                _toastNotification.AddSuccessToastMessage("Configuración guardada con éxito.");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Ocurrió un error inesperado.");
                return View(model);
            }
        }
        #endregion
    }
}
