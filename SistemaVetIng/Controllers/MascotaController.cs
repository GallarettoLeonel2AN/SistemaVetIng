using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class MascotaController : Controller
    {
        private readonly IMascotaService _mascotaService;
        private readonly IClienteService _clienteService;
        private readonly IToastNotification _toastNotification;
        private readonly ITurnoService _turnoService;
        private readonly IChipService _chipService;
        private readonly UserManager<Usuario> _userManager;


        private readonly List<string> _razasPeligrosas = new List<string>
        {
            "pitbull", "rottweiler", "dogo argentino", "fila brasileiro",
            "akita inu", "tosa inu", "doberman", "staffordshire bull terrier",
            "american staffordshire terrier", "pastor alemán"
        };

        public MascotaController(
            IMascotaService mascotaService,
            IClienteService clienteService, 
            IToastNotification toastNotification,
            ITurnoService turnoService,
            UserManager<Usuario> userManager,
            IChipService chipService)
        {

            _mascotaService = mascotaService;
            _clienteService = clienteService;
            _toastNotification = toastNotification;
            _turnoService = turnoService;
            _userManager = userManager;
            _chipService = chipService;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Detalle(int id)
        {
            if (id <= 0)
            {
                _toastNotification.AddErrorToastMessage("ID de mascota inválido.");
                return RedirectToAction("PaginaPrincipal", "Cliente");
            }


            var viewModel = await _mascotaService.ObtenerDetalleConHistorial(id);

            if (viewModel == null)
            {
                _toastNotification.AddErrorToastMessage("Mascota no encontrada.");
                return RedirectToAction("PaginaPrincipal", "Cliente");
            }


            if (User.IsInRole("Cliente"))
            {
                var userName = User.Identity.Name;

                var clienteActual = await _clienteService.ObtenerClientePorUserNameAsync(userName);

                if (clienteActual == null || !viewModel.PropietarioNombreCompleto.Contains(clienteActual.Nombre))
                {
                    _toastNotification.AddWarningToastMessage("Acceso denegado. No es el propietario de esta mascota.");
                    return RedirectToAction("PaginaPrincipal", "Cliente");
                }
            }

            return View(viewModel);
        }


        #region LISTAR CLIENTES

        [HttpGet]
        public async Task<IActionResult> ListarClientes(string busquedaCliente = null, int page = 1)
        {
            int pageSize = 6; 
            var clientesPaginados = await _clienteService.ListarPaginadoAsync(page, pageSize, busquedaCliente);

            var viewModel = new ListadoClientesViewModel
            {
                ClientesPaginados = clientesPaginados,
                BusquedaActual = busquedaCliente 
            };

            return View(viewModel);
        }
        #endregion

        #region REGISTRAR MASCOTA

        [HttpGet]

        public async Task<IActionResult> RegistrarMascota(int clienteId, int? turnoIdParaRedireccion = null)
        {
            var cliente = await _clienteService.ObtenerPorId(clienteId);
            if (cliente == null)
            {
                _toastNotification.AddInfoToastMessage("El Cliente no fue encontrado.");
                return RedirectToAction(nameof(ListarClientes)); 
            }

            ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";

            var model = new MascotaRegistroViewModel
            {
                ClienteId = clienteId,
                TurnoIdParaRedireccion = turnoIdParaRedireccion
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarMascota(MascotaRegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }

            // Auditoria
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Si no podemos identificar al usuario, no permitimos la acción
                _toastNotification.AddErrorToastMessage("Error de autenticación. No se pudo registrar la mascota.");
                return View(model);
            }

            int auditUserId = user.Id;
            string auditUserName = user.UserName;

            var roles = await _userManager.GetRolesAsync(user);
            string rolUsuario = roles.FirstOrDefault() ?? "Sin Rol";


            var (nuevaMascota, success, message) = await _mascotaService.Registrar(
                model,
                auditUserId,
                auditUserName,
                rolUsuario
            );

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(message);

                if (model.TurnoIdParaRedireccion.HasValue)
                {
                    var turnoOriginal = await _turnoService.ObtenerPorIdConDatosAsync(model.TurnoIdParaRedireccion.Value);
                    if (turnoOriginal != null)
                    {
                        turnoOriginal.MascotaId = nuevaMascota.Id;
                        _turnoService.Actualizar(turnoOriginal); 
                        await _turnoService.Guardar(); 
                    }

                    return RedirectToAction("RegistrarAtencionConTurno", "AtencionVeterinaria", new { turnoId = model.TurnoIdParaRedireccion.Value });
                }

                return RedirectToAction(nameof(ListarClientes));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(message);
                // Recargar datos para la vista de error
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }
        }
        #endregion

        #region MODIFICAR MASCOTA

        [HttpGet]
        public async Task<IActionResult> ModificarMascota(int? id)
        {
            if (id == null)
            {
                _toastNotification.AddErrorToastMessage("No se pudo encontrar la mascota. ID no proporcionado.");
                return RedirectToAction(nameof(ListarClientes));
            }

            var mascota = await _mascotaService.ObtenerPorId(id.Value);

            if (mascota == null)
            {
                _toastNotification.AddErrorToastMessage("La mascota que intenta editar no existe.");
                return RedirectToAction(nameof(ListarClientes));
            }

            bool tieneChipReal = await _chipService.PoseeChipMascota(mascota.Id);

            var viewModel = new MascotaEditarViewModel
            {
                Id = mascota.Id,
                ClienteId = mascota.ClienteId,
                Nombre = mascota.Nombre,
                Especie = mascota.Especie,
                Raza = mascota.Raza,
                FechaNacimiento = mascota.FechaNacimiento,
                Sexo = mascota.Sexo,
                RazaPeligrosa = mascota.RazaPeligrosa,
                Chip = tieneChipReal
            };

            var cliente = await _clienteService.ObtenerPorId(mascota.ClienteId);
            if (cliente != null)
            {
                ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarMascota(MascotaEditarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                _toastNotification.AddErrorToastMessage("Hubo un error con los datos proporcionados.");
                return View(model);
            }

            try
            {
                var (success, message) = await _mascotaService.Modificar(model);

                if (success)
                {
                    _toastNotification.AddSuccessToastMessage(message);
                    return RedirectToAction(nameof(ListarClientes)); 
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(message);
                    var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                    if (cliente != null)
                    {
                        ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error inesperado: {ex.Message}");
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }
        }
        #endregion

        #region ELIMINAR MASCOTA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMascota(int? id)
        {

            if (id == null)
            {
                _toastNotification.AddErrorToastMessage("No se pudo eliminar la mascota. ID no proporcionado.");
                if (User.IsInRole("Veterinario"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinario");
                }
                else
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                }
                ;
            }

            var (success, message) = await _mascotaService.Eliminar(id.Value);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(message);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(message);
            }

            if (User.IsInRole("Veterinario"))
            {
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }
            else
            {
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
            ;
        }
        #endregion


    }
}