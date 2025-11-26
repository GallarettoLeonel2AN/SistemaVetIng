using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinaria,Veterinario")]
    public class AuditoriaController : Controller
    {
        private readonly IAuditoriaService _auditoriaService;

        public AuditoriaController(IAuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? busquedaUsuario, string? tipoEventoSeleccionado, DateTime? fechaInicio, DateTime? fechaFin, int page = 1)
        {
            int pageSize = 20; 

            var eventosPaginados = await _auditoriaService.ObtenerLogPaginadoAsync(
                page,
                pageSize,
                busquedaUsuario,
                tipoEventoSeleccionado,
                fechaInicio,
                fechaFin
            );

            var viewModel = new AuditoriaViewModel
            {
                EventosPaginados = eventosPaginados,
                BusquedaUsuario = busquedaUsuario,
                TipoEventoSeleccionado = tipoEventoSeleccionado,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };


            return View(viewModel);
        }
    }
}
