using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;


namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class HistoriaClinicaController : Controller
    {
        private readonly IHistoriaClinicaService _historiaClinicaService;
        private readonly IToastNotification _toastNotification;
        private readonly IClienteService _clienteService;
        public HistoriaClinicaController(IHistoriaClinicaService historiaClinicaService,
            IToastNotification toastNotification,
            IClienteService clienteService)
        {
            _historiaClinicaService = historiaClinicaService;
            _toastNotification = toastNotification;
            _clienteService = clienteService;
        }


        #region LISTADO MASCOTAS DEL CLIENTE
        public async Task<IActionResult> MascotasCliente(int clienteId)
        {
            var cliente = await _historiaClinicaService.GetMascotasCliente(clienteId);

            if (cliente == null)
            {
                _toastNotification.AddErrorToastMessage("El cliente no existe!");
                return RedirectToAction(nameof(BuscarClienteParaSeguimiento));
            }

            return View(cliente);
        }
        #endregion

        #region DETALLES DE HISTORIAS CLINICAS
        public async Task<IActionResult> DetalleHistoriaClinica(int mascotaId)
        {
            var mascota = await _historiaClinicaService.GetDetalleHistoriaClinica(mascotaId);

            if (mascota == null)
            {
                return RedirectToAction(nameof(BuscarClienteParaSeguimiento));
            }
            if (mascota.HistoriaClinica == null)
            {
                return RedirectToAction("MascotasCliente", new { clienteId = mascota.Propietario.Id });
            }

            return View(mascota);
        }
        #endregion

        #region BUSCAR CLIENTE PARA SEGUIMIENTOS
        [HttpGet]
        public async Task<IActionResult> BuscarClienteParaSeguimiento(string busquedaCliente = null, int page = 1)
        {
            int pageSize = 10;

            var clientesPaginados = await _clienteService.ListarPaginadoAsync(page, pageSize, busquedaCliente);

            var viewModel = new ListadoClientesViewModel
            {
                ClientesPaginados = clientesPaginados,
                BusquedaActual = busquedaCliente
            };

            return View(viewModel);
        }
        #endregion
    }
}