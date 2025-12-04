using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Models;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;
using X.PagedList;

namespace SistemaVetIng.Controllers
{

    public class ClienteController : Controller
    {

        private readonly IToastNotification _toastNotification;
        private readonly IClienteService _clienteService;
        private readonly ITurnoService _turnoService;
        private readonly IMascotaService _mascotaService;
        private readonly IAtencionVeterinariaService _atencionVeterinariaService;
        private readonly IPagoService _pagoService;


        public ClienteController(IToastNotification toastNotification, 
            IClienteService clienteService,
            ITurnoService turnoService,
            IMascotaService mascotaService,
            IAtencionVeterinariaService atencionVeterinariaService,
            IPagoService pagoService)
        {
            _toastNotification = toastNotification;
            _clienteService = clienteService;
            _turnoService = turnoService;
            _mascotaService = mascotaService;
            _atencionVeterinariaService = atencionVeterinariaService;
            _pagoService = pagoService;
        }


        #region PAGINA PRINCIPAL
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal(string busquedaTurno = null, int page = 1)
        {

            var usuarioIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(usuarioIdString, out int usuarioIdNumerico))
            {
                return BadRequest("El formato ID de usuario no es valido");
            }
            var userName = User.Identity.Name;

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Account");
            }

            var cliente = await _clienteService.ObtenerClientePorUserNameAsync(userName);

            if (cliente == null)
            {
                // Redirigir si no se encuentra el cliente con el userName
                return RedirectToAction("Login", "Account");
            }

            int pageSizeTurnos = 5;
            var turnosPaginados = await _turnoService.ListarPaginadoPorClienteAsync(cliente.Id, page, pageSizeTurnos, busquedaTurno);
            var mascotas = await _mascotaService.ObtenerMascotasPorClienteUserNameAsync(userName);
            var pagosPendientes = await _atencionVeterinariaService.ObtenerPagosPendientesPorClienteId(cliente.Id);

            var viewModel = new ClientePaginaPrincipalViewModel
            {
                NombreCompleto = $"{cliente.Nombre} {cliente.Apellido}",
                BusquedaTurnoActual = busquedaTurno,
                PaginacionTurnos = turnosPaginados,
                Mascotas = mascotas.ToList(),
                PagosPendientes = pagosPendientes 
            };


            if (turnosPaginados != null && turnosPaginados.Any())
            {
                viewModel.Turnos = turnosPaginados.Select(t => new TurnoViewModel
                {
                    Id = t.Id,
                    Fecha = t.Fecha,
                    Horario = t.Horario,
                    Estado = t.Estado,
                    Motivo = t.Motivo,
                    PrimeraCita = t.PrimeraCita,
                    Mascota = t.Mascota, 
                }).ToList(); 
            }


            // Obtener ID del usuario logueado
            var usuarioIdString2 = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdString2, out int usuarioIdNumerico2))
            {
                return Unauthorized("No se pudo obtener el ID del usuario.");
            }
            var clienteLogueado = await _clienteService.ObtenerPorIdUsuario(usuarioIdNumerico2);
            if (clienteLogueado == null)
            {
                return NotFound("No se encontró un perfil de cliente para este usuario.");
            }

            int clienteId = clienteLogueado.Id;


            // Datos para Reportes Dashboard

            viewModel.CantidadTurnos = await _turnoService.ContarTotalTurnosClienteAsync(clienteId);
            viewModel.CantidadTurnosPendientesPorCliente = await _turnoService.CantidadTurnosPendientesPorCliente(clienteId);
            viewModel.CantidadMascotasPorCliente = await _mascotaService.ContarTotalMascotasPorClienteAsync(clienteId);
            viewModel.CantidadPagosPendientes = await _atencionVeterinariaService.CantidadPagosPendientes(clienteId);

            return View(viewModel);
        }
        #endregion


        #region REGISTRAR CLIENTE
        [HttpGet]
        public IActionResult RegistrarCliente()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Prevenir ataques de falsificación de solicitudes
        public async Task<IActionResult> RegistrarCliente(ClienteRegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Hubo errores al registrar el cliente.");
                return View(model);
            }

            try
            {
                await _clienteService.Registrar(model);
                _toastNotification.AddSuccessToastMessage("¡Cliente registrado correctamente!");
                
                if (User.IsInRole("Veterinario"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinario");
                }
                else if (User.IsInRole("Veterinaria"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
                return View(model);
            }
        }
        #endregion


        #region MODIFICAR CLIENTE
        [HttpGet]
        public async Task<IActionResult> ModificarCliente(int id)
        {

            var cliente = await _clienteService.ObtenerPorId(id);

            if (cliente == null)
            {
                _toastNotification.AddErrorToastMessage("Cliente no encontrado.");
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

            var viewModel = new ClienteEditarViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Dni = cliente.Dni,
                Email = cliente.Usuario?.UserName,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarCliente(ClienteEditarViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _clienteService.Modificar(model);
                _toastNotification.AddSuccessToastMessage("¡Cliente actualizado correctamente!");

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
            catch (KeyNotFoundException)
            {
                _toastNotification.AddErrorToastMessage("Cliente no encontrado.");
                return View(model);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
                return View(model);
            }

        }
        #endregion


        #region ELIMINAR CLIENTE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            try
            {
                await _clienteService.Eliminar(id);
                _toastNotification.AddSuccessToastMessage("El cliente ha sido eliminado exitosamente.");
            }
            catch (KeyNotFoundException)
            {
                _toastNotification.AddErrorToastMessage("El cliente que intenta eliminar no existe.");
            }
            catch (DbUpdateException)
            {
                _toastNotification.AddErrorToastMessage("No se pudo eliminar el cliente. Hay registros asociados.");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
            }

            if (User.IsInRole("Veterinario"))
            {
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }
            else
            {
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
        }
        #endregion


        #region HISTORIAL DE PAGOS
        [HttpGet]
        public async Task<IActionResult> HistorialPagos(int page = 1)
        {
            int pageSize = 5;

            // Obtener ID del usuario logueado
            var usuarioIdString2 = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdString2, out int usuarioIdNumerico2))
            { 
                _toastNotification.AddErrorToastMessage("No se pudo obtener el ID del usuario.");
            }
            var clienteLogueado = await _clienteService.ObtenerPorIdUsuario(usuarioIdNumerico2);
            if (clienteLogueado == null)
            {
                _toastNotification.AddErrorToastMessage("No se encontró un perfil de cliente para este usuario.");
            }

            int clienteId = clienteLogueado.Id;

            if (clienteId == 0)
            {
                return Unauthorized();
            }
            var pagos = await _pagoService.ListarHistorialPagos(clienteId, page, pageSize);

            var pagosViewModel = pagos.Select(pago => new PagosItemViewModel
            {
                Fecha = pago.Fecha,
                Monto = pago.MontoTotal,
                MetodoDePago = pago.MetodoPagoId switch // Tipo
                {
                    1 => "Efectivo",
                    2 => "Pago Online / MercadoPago",
                    3 => "Tarjeta",
                    _ => "Desconocido"
                },
                IconoCssClass = pago.MetodoPagoId switch // Iconos
                {
                    1 => "fa-solid fa-money-bill-wave",
                    2 => "fa-solid fa-mobile-screen-button",
                    3 => "fa-regular fa-credit-card",
                    _ => "fa-solid fa-circle-question"
                }
            });

            var viewModel = new HistorialPagosViewModel
            {
                // Convertimos la lista mapeada de vuelta a IPagedList
                PagosPaginados = new StaticPagedList<PagosItemViewModel>(pagosViewModel, pagos)
            };

            return View(viewModel);
        }

        #endregion
    }
}