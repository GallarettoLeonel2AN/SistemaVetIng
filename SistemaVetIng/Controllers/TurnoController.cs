using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Globalization;

namespace SistemaVetIng.Controllers
{
    public class TurnoController : Controller
    {
        private readonly IToastNotification _toastNotification;
        private readonly IMascotaService _mascotaService;
        private readonly ITurnoService _turnoService;
        private readonly IClienteService _clienteService;
        private readonly UserManager<Usuario> _userManager;

        public TurnoController(
            IToastNotification toastNotification,
            IMascotaService mascotaService,
            UserManager<Usuario> userManager,
            IClienteService clienteService,
            ITurnoService turnoService)
        {
            _toastNotification = toastNotification;
            _mascotaService = mascotaService;
            _userManager = userManager;
            _turnoService = turnoService;
            _clienteService = clienteService;
        }

        #region RESERVAR TURNO

        [HttpGet]
        public async Task<IActionResult> ReservarTurno()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cliente = await _clienteService.ObtenerPorIdUsuario(usuarioActual.Id);
            if (cliente == null)
            {
                _toastNotification.AddWarningToastMessage("Debe completar su perfil de cliente para reservar un turno.");

                return RedirectToAction("RegistrarCliente", "Cliente");
            }

            var mascotasDelCliente = (await _mascotaService.ListarMascotasPorClienteId(cliente.Id)).ToList();

            var viewModel = new ReservaTurnoViewModel
            {
                Mascotas = mascotasDelCliente,
                HasMascotas = mascotasDelCliente.Any(),
                // Si NO tiene mascotas !mascotasDelCliente.Any(), PrimeraCita true
                PrimeraCita = !mascotasDelCliente.Any()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(ReservaTurnoViewModel model)
        {
            // No permitir fechas pasadas
            if (model.Fecha.Date < DateTime.Today)
            {
                return Json(new { success = false, message = "No se puede reservar un turno para una fecha pasada." });
            }


            // Lógica para manejar Primera Cita
            if (model.PrimeraCita)
            {
                model.MascotaId = null;
            }
            else
            {
                if (model.MascotaId == null || model.MascotaId == 0)
                {
                    ModelState.AddModelError("MascotaId", "Debe seleccionar una mascota si no es la primera cita.");
                }
            }

            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return Json(new { success = false, message = "Usuario no autenticado." });
            }

            var cliente = await _clienteService.ObtenerPorIdUsuario(usuarioActual.Id);
            if (cliente == null)
            {
                return Json(new { success = false, message = "No se encontró el perfil de cliente." });
            }

            model.ClienteId = cliente.Id;


            try
            {
                await _turnoService.ReservarTurnoAsync(model);
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("PaginaPrincipal", "Cliente")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }


        }

        #endregion

        #region OBTENER HORARIOS
        [HttpGet]
        public async Task<IActionResult> ObtenerHorariosDisponibles(string fecha)
        {
            const string formatoFecha = "yyyy-MM-dd";
            DateTime fechaSeleccionada;

            if (!DateTime.TryParseExact(fecha, formatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaSeleccionada))
            {
                return Json(new List<string>());
            }

            // Si la fecha es pasada, regresamos lista vacía sin llamar al servicio
            if (fechaSeleccionada.Date < DateTime.Today)
            {
                return Json(new List<string>());
            }

            var horarios = await _turnoService.GetHorariosDisponiblesAsync(fechaSeleccionada);
            return Json(horarios);
        }

        #endregion

        #region CANCELAR TURNOS

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")] 
        public async Task<IActionResult> CancelarTurnoCliente(int turnoId)
        {
            var (success, message) = await _turnoService.CancelarTurnoAsync(turnoId, User);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(message);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(message);
            }

            return RedirectToAction("PaginaPrincipal", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Veterinario")]
        public async Task<IActionResult> CancelarTurnoAdmin(int turnoId)
        {
            var (success, message) = await _turnoService.CancelarTurnoAsync(turnoId, User);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(message);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(message);
            }

            return RedirectToAction("PaginaPrincipal", "Veterinario");
        }

        #endregion

        #region MARCAR AUSENCIA

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Veterinario")]
        public async Task<IActionResult> MarcarNoAsistio(int turnoId)
        {
            var turno = await _turnoService.ObtenerPorIdConDatosAsync(turnoId); 

            if (turno == null)
            {
                _toastNotification.AddErrorToastMessage("Turno no encontrado.");
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }

            if (turno.Estado != "Pendiente")
            {
                _toastNotification.AddWarningToastMessage($"El turno ya está en estado '{turno.Estado}'.");
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }

            turno.Estado = "No Asistió";

            try
            {
                _turnoService.Actualizar(turno);
                await _turnoService.Guardar();
                _toastNotification.AddInfoToastMessage("Turno marcado como 'No Asistió'.");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error al actualizar el estado del turno.");
            }

            return RedirectToAction("PaginaPrincipal", "Veterinario");
        }

        #endregion

        #region OBTENER TURNOS POR DIA SELECCIONADO

        [HttpGet]
        public async Task<IActionResult> TurnosPorDiaSeleccionado(DateTime? fechaSeleccionada)
        {
            DateTime fecha = fechaSeleccionada ?? DateTime.Now;

            var turnos = await _turnoService.ObtenerTurnosPorFechaAsync(fecha);

            var viewModel = new TurnosPorDiaViewModel
            {
                FechaSeleccionada = fecha,
                TurnosDelDia = turnos.Select(t => new TurnoViewModel
                {
                    Id = t.Id,
                    Horario = t.Horario,
                    Motivo = t.Motivo,
                    Estado = t.Estado,
                    PrimeraCita = t.PrimeraCita,
                    NombreMascota = t.Mascota?.Nombre, 
                    NombreCliente = $"{t.Cliente?.Nombre} {t.Cliente?.Apellido}", 
                    ClienteId = t.ClienteId, 
                    MascotaId = t.MascotaId 
                }).OrderBy(tpd => tpd.Horario).ToList()
            };

            return View(viewModel);
        }

        #endregion
    }
}