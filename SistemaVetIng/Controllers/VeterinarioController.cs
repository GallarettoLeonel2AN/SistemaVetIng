using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Models;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class VeterinarioController : Controller
    {
        private readonly IToastNotification _toastNotification;
        private readonly IVeterinarioService _veterinarioService;
        private readonly IClienteService _clienteService;
        private readonly IMascotaService _mascotaService;
        private readonly ITurnoService _turnoService;
        private readonly IAtencionVeterinariaService _atencionVeterinariaService;
        private readonly IVeterinariaConfigService _veterinariaConfigService;

        public VeterinarioController(
            IVeterinarioService veterinarioService,
            IToastNotification toastNotification,
            IClienteService clienteService,
            IMascotaService mascotaService,
            ITurnoService turnoService,
            IAtencionVeterinariaService atencionVeterinariaService,
            IVeterinariaConfigService veterinariaConfigService)
        {
            _veterinarioService = veterinarioService;
            _toastNotification = toastNotification;
            _clienteService = clienteService;
            _mascotaService = mascotaService;
            _turnoService = turnoService;
            _atencionVeterinariaService = atencionVeterinariaService;
            _veterinariaConfigService = veterinariaConfigService;
        }

        #region PAGINA PRINCIPAL
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal(
            string busquedaCliente = null,
            string busquedaMascota = null,
            string busquedaVeterinario = null,
            int page = 1,
            int pageMascota = 1,
            int pageVet = 1)
        {


            var viewModel = new VeterinarioPaginaPrincipalViewModel();
            int pageSizeClientes = 3;
            int pageSizeMascotas = 3;
            int pageSizeVeterinarios = 3;

            //  Cargar ConfiguracionHoraria SI SE LE DAN PERMISOS

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

            // Cargar Veterinarios en las tablas

            var veterinariosPaginados = await _veterinarioService.ListarPaginadoAsync(pageVet, pageSizeVeterinarios, busquedaVeterinario);

            viewModel.Veterinarios = veterinariosPaginados.Select(v => new VeterinarioViewModel
            {
                Id = v.Id,
                NombreCompleto = $"{v.Nombre} {v.Apellido}",
                Telefono = v.Telefono,
                NombreUsuario = v.Usuario?.Email,
                Direccion = v.Direccion,
                Matricula = v.Matricula,
            }).ToList();

            viewModel.PaginacionVeterinarios = veterinariosPaginados;



            // CARGA DE CLIENTES
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

            // CARGA DE MASCOTAS 

            var mascotasPaginadas = await _mascotaService.ListarPaginadoAsync(pageMascota, pageSizeMascotas, busquedaMascota);

            viewModel.Mascotas = mascotasPaginadas.Select(m => new MascotaListViewModel
            {
                Id = m.Id,
                NombreMascota = m.Nombre,
                Especie = m.Especie,
                Sexo = m.Sexo,
                Raza = m.Raza,
                EdadAnios = DateTime.Today.Year - m.FechaNacimiento.Year - (DateTime.Today.DayOfYear < m.FechaNacimiento.DayOfYear ? 1 : 0),
                NombreDueno = $"{m.Propietario?.Nombre} {m.Propietario?.Apellido}",
                ClienteId = m.Propietario?.Id ?? 0
            }).ToList();


            viewModel.PaginacionMascotas = mascotasPaginadas;


            // CARGA DE CITAS DE HOY
            var turnosDeHoy = await _turnoService.ObtenerTurnosPorFechaAsync(DateTime.Today);

            if (turnosDeHoy != null)
            {
                // Si la lista NO es nula, la procesamos
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
            }
            else
            {
                // Si la lista ES nula (porque no hay turnos),
                // le asignamos una LISTA VACÍA.
                viewModel.CitasDeHoy = new List<TurnoViewModel>();
            }


            // Obtener ID del veterinario logueado
            var usuarioIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdString, out int usuarioIdNumerico))
            {
                return Unauthorized("No se pudo obtener el ID del usuario.");
            }
            var veterinarioLogueado = await _veterinarioService.ObtenerPorIdUsuario(usuarioIdNumerico);
            if (veterinarioLogueado == null)
            {
                return NotFound("No se encontró un perfil de veterinario para este usuario.");
            }

            int veterinarioId = veterinarioLogueado.Id;


            // Datos para Reportes dashboard
            viewModel.Nombre = $"{veterinarioLogueado.Nombre} {veterinarioLogueado.Apellido}";
            viewModel.CantidadCitasHoy = await _turnoService.ContarTurnosParaFechaAsync(DateTime.Today);
            viewModel.CantidadAtencionesPorVeterinario = await _atencionVeterinariaService.CantidadAtencionesPorVeterinario(veterinarioId);
            viewModel.MascotaMasFrecuentePorVeterinario = await _atencionVeterinariaService.ObtenerMascotaMasFrecuentePorVeterinario(veterinarioId);

            return View(viewModel);
        }
        #endregion

        #region REGISTRAR VETERINARIO

        [HttpGet]
        public IActionResult RegistrarVeterinario()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarVeterinario(VeterinarioRegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Hubo errores al registrar el veterinario.");
                return View(model);
            }

            try
            {
                await _veterinarioService.Registrar(model);
                _toastNotification.AddSuccessToastMessage("¡Veterinario registrado correctamente!");
                if (User.IsInRole("Veterinaria"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                }
                else
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinario");
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
                return View(model);
            }
        }
        #endregion

        #region MODIFICAR VETERINARIO
        [HttpGet]
        public async Task<IActionResult> ModificarVeterinario(int id)
        {
            var veterinario = await _veterinarioService.ObtenerPorId(id);
            if (veterinario == null)
            {
                _toastNotification.AddErrorToastMessage("Veterinario no encontrado.");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }

            // Mapeo manual de la entidad al ViewModel para mostrarlo en el formulario de Modificar
            var viewModel = new VeterinarioEditarViewModel
            {
                Id = veterinario.Id,
                Nombre = veterinario.Nombre,
                Apellido = veterinario.Apellido,
                Dni = veterinario.Dni,
                Email = veterinario.Usuario?.Email,
                Direccion = veterinario.Direccion,
                Telefono = veterinario.Telefono,
                Matricula = veterinario.Matricula
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarVeterinario(VeterinarioEditarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _veterinarioService.Modificar(model);
                _toastNotification.AddSuccessToastMessage("¡Veterinario actualizado correctamente!");
                if (User.IsInRole("Veterinaria"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                }
                else
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinario");
                }
            }
            catch (KeyNotFoundException)
            {
                _toastNotification.AddErrorToastMessage("Veterinario no encontrado.");
                return View(model);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
                return View(model);
            }
        }
        #endregion

        #region ELIMINAR VETERINARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarVeterinario(int id)
        {
            try
            {
                await _veterinarioService.Eliminar(id);
                _toastNotification.AddSuccessToastMessage("El veterinario ha sido eliminado exitosamente.");
            }
            catch (KeyNotFoundException)
            {
                _toastNotification.AddErrorToastMessage("El veterinario que intenta eliminar no existe.");
            }
            catch (DbUpdateException)
            {
                _toastNotification.AddErrorToastMessage("No se pudo eliminar el veterinario. Hay registros asociados.");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
            }

            if (User.IsInRole("Veterinaria"))
            {
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
            else
            {
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }
        }
        #endregion

    }
}
