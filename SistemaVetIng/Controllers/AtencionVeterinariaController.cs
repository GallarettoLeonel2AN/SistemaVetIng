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
        // En AtencionVeterinariaController.cs

        [HttpGet]
        public async Task<IActionResult> Historial(int id)
        {
            // 1. Buscamos la lista de mementos (versiones viejas) usando el servicio
            var historial = await _atencionService.ObtenerHistorialAsync(id);

            // 2. También necesitamos saber datos básicos de la atención actual (para el título)
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
                // 1. Llamamos a la lógica de restauración del servicio
                await _atencionService.RestaurarVersionAsync(mementoId);

                // 2. Buscamos el memento para saber a qué historia clínica volver
                // (Esto es un truco rápido para redirigir bien, podrías optimizarlo)
                // Lo ideal es que RestaurarVersionAsync devuelva el ID de la HistoriaClinica

                _toastNotification.AddSuccessToastMessage("¡Versión restaurada con éxito!");

                // Redirigimos al usuario a la página anterior (el navegador maneja esto)
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
            // Necesitamos un método en el servicio que busque la atención por ID 
            // y la convierta en tu ViewModel. (Te paso el código de esto más abajo)
            var model = await _atencionService.ObtenerAtencionParaEditarAsync(id);

            if (model == null) return NotFound();

            return View(model);
        }

        // 2. POST: Recibe los cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Editar(AtencionVeterinariaViewModel model, string motivoCambio)
        {
            if (!ModelState.IsValid)
            {
                // Si hay error, recargamos listas (si las usas) y volvemos a la vista
                return View(model);
            }

            try
            {
                // Guardamos los cambios y el memento
                await _atencionService.EditarAtencionConRespaldoAsync(model, User, motivoCambio);

                _toastNotification.AddSuccessToastMessage("Cambios guardados correctamente.");

                // --- AQUÍ ESTÁ LA CORRECCIÓN ---
                // Redirige a la Historia Clínica de la mascota específica
                return RedirectToAction("DetalleHistoriaClinica", "HistoriaClinica", new { mascotaId = model.MascotaId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al editar: " + ex.Message);
                return View(model);
            }
        }
        #endregion
    }
}