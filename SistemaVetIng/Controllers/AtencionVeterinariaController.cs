using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;


namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class AtencionVeterinariaController : Controller
    {
        private readonly IAtencionVeterinariaService _atencionService;
        private readonly IToastNotification _toastNotification;
        private readonly IHistoriaClinicaService _historiaClinicaService;
        private readonly ITurnoService _turnoService;
        private readonly IVacunaService _vacunaService;
        private readonly IEstudioService _estudioService;

        public AtencionVeterinariaController(IAtencionVeterinariaService atencionService,
            IToastNotification toastNotification,
            IHistoriaClinicaService historiaClinicaService,
            ITurnoService turnoService,
            IVacunaService vacunaService,
            IEstudioService estudioService)
        {
            _atencionService = atencionService;
            _toastNotification = toastNotification;
            _historiaClinicaService = historiaClinicaService;
            _turnoService = turnoService;
            _vacunaService = vacunaService;
            _estudioService = estudioService;
        }

        #region REGISTRAR ATENCION SINTURNO
        [HttpGet]
        public async Task<IActionResult> RegistrarAtencion(int historiaClinicaId)
        {
            var viewModel = await _atencionService.GetAtencionVeterinariaViewModel(historiaClinicaId);

            if (viewModel == null)
            {
                _toastNotification.AddErrorToastMessage("Historia Clinica no encontrada!");
                return RedirectToAction("ListaClientesParaSeguimiento", "HistoriaClinica");
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarAtencion(AtencionVeterinariaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si la validación falla, volvemos a cargar los datos necesarios para la vista
                var viewModel = await _atencionService.GetAtencionVeterinariaViewModel(model.HistoriaClinicaId);
                if (viewModel != null)
                {
                    viewModel.Diagnostico = model.Diagnostico;
                    viewModel.PesoKg = model.PesoKg;
                    viewModel.Medicamento = model.Medicamento;
                    viewModel.Dosis = model.Dosis;
                    viewModel.Frecuencia = model.Frecuencia;
                    viewModel.DuracionDias = model.DuracionDias;
                    viewModel.ObservacionesTratamiento = model.ObservacionesTratamiento;
                    viewModel.VacunasSeleccionadasIds = model.VacunasSeleccionadasIds;
                    viewModel.EstudiosSeleccionadosIds = model.EstudiosSeleccionadosIds;
                }
                return View(viewModel);
            }

            var result = await _atencionService.CreateAtencionVeterinaria(model, User);

            if (result != null)
            {
                _toastNotification.AddErrorToastMessage("Error al crear la atencion!");
                var viewModel = await _atencionService.GetAtencionVeterinariaViewModel(model.HistoriaClinicaId);
                return View(viewModel);
            }

            _toastNotification.AddSuccessToastMessage("Atencion creada correctamente!");
            // Obtener el Id de la mascota desde la base de datos
            var historiaClinica = await _historiaClinicaService.GetDetalleHistoriaClinica(model.HistoriaClinicaId);
            if (historiaClinica != null)
            {
                return RedirectToAction("DetalleHistoriaClinica", "HistoriaClinica", new { mascotaId = historiaClinica.Id});
            }
            return RedirectToAction("ListaClientesParaSeguimiento", "HistoriaClinica");
        }
        #endregion

        #region REGISTRAR ATENCION TURNO

        [HttpGet]
        public async Task<IActionResult> RegistrarAtencionConTurno(int turnoId)
        {

            var todasLasVacunas = await _vacunaService.ListarTodo();
            var todosLosEstudios = await _estudioService.ListarTodo();


            var turno = await _turnoService.ObtenerPorIdConDatosAsync(turnoId);
            if (turno == null)
            {
                _toastNotification.AddErrorToastMessage("El turno no fue encontrado.");
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }

            if (turno.PrimeraCita && turno.MascotaId == null)
            {
                _toastNotification.AddInfoToastMessage("Es una primera cita. Por favor, registra primero la mascota.");
               return RedirectToAction("RegistrarMascota", "Mascota", new { clienteId = turno.ClienteId, turnoIdParaRedireccion = turno.Id });
            }

            var historiaClinica = await _historiaClinicaService.ObtenerPorMascotaIdAsync(turno.MascotaId.Value);
            if (historiaClinica == null)
            {
                _toastNotification.AddErrorToastMessage("No se encontró la historia clínica para esta mascota.");
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }


            var viewModel = new AtencionPorTurnoViewModel
            {
                TurnoId = turno.Id,
                MascotaId = turno.MascotaId.Value,
                NombreMascota = turno.Mascota.Nombre,
                NombreCliente = $"{turno.Cliente.Nombre} {turno.Cliente.Apellido}",
                HistoriaClinicaId = historiaClinica.Id,
                VacunasDisponibles = new SelectList(todasLasVacunas, "Id", "Nombre"),
                EstudiosDisponibles = new SelectList(todosLosEstudios, "Id", "Nombre")
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarAtencionConTurno(AtencionPorTurnoViewModel model)
        {
            var todasLasVacunas = await _vacunaService.ListarTodo();
            var todosLosEstudios = await _estudioService.ListarTodo();

            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Por favor, corrige los errores del formulario.");
                model.VacunasDisponibles = new SelectList(todasLasVacunas, "Id", "Nombre");
                model.EstudiosDisponibles = new SelectList(todosLosEstudios, "Id", "Nombre");
                return View("RegistrarAtencionConTurno", model);
            }

            try
            {
                await _atencionService.RegistrarAtencionDesdeTurnoAsync(model, User);

                _toastNotification.AddSuccessToastMessage("Atención registrada y turno finalizado con éxito.");
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Ocurrió un error inesperado al guardar la atención.");
                model.VacunasDisponibles = new SelectList(todasLasVacunas, "Id", "Nombre");
                model.EstudiosDisponibles = new SelectList(todosLosEstudios, "Id", "Nombre");
                return View("RegistrarAtencionConTurno", model);
            }
        }
        #endregion

        #region MEMENTO

        [HttpGet]
        public async Task<IActionResult> Historial(int id)
        {
            // Buscar la lista de mementos (versiones viejas)
            var historial = await _atencionService.ObtenerHistorialAsync(id);

            var atencionActual = await _atencionService.ObtenerPorId(id);
            ViewData["AtencionId"] = id;
            ViewData["MascotaNombre"] = atencionActual.HistoriaClinica.Mascota.Nombre;

            return View(historial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restaurar(int mementoId)
        {
            try
            {
                await _atencionService.RestaurarVersionAsync(mementoId);

                _toastNotification.AddSuccessToastMessage("¡Versión restaurada con éxito!");

                return Redirect(Request.Headers["Referer"].ToString());
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error al restaurar: " + ex.Message);
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var model = await _atencionService.ObtenerAtencionParaEditarAsync(id);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Editar(AtencionVeterinariaViewModel model, string motivoCambio)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _atencionService.EditarAtencionConRespaldoAsync(model, User, motivoCambio);
                _toastNotification.AddSuccessToastMessage("Cambios guardados correctamente.");

             
                return RedirectToAction("DetalleHistoriaClinica", "HistoriaClinica", new { mascotaId = model.MascotaId });
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error: " + ex.Message);

                // Recargamos listas para que el dropdown no falle al volver a la vista
                var vacunas = await _vacunaService.ListarTodo();
                var estudios = await _estudioService.ListarTodo();
                model.VacunasDisponibles = new SelectList(vacunas, "Id", "Nombre");
                model.EstudiosDisponibles = new SelectList(estudios, "Id", "Nombre");

                return View(model);
            }
        }
        #endregion
    }
}