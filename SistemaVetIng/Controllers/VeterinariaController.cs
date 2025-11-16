using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Extension;
using SistemaVetIng.Models.Indentity;
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
        private readonly IAtencionVeterinariaService _atencionVeterinariaService;
        private readonly IPermissionService _permissionService;
        private readonly RoleManager<Rol> _roleManager;
        private readonly UserManager<Usuario> _userManager;


        public VeterinariaController(
            IVeterinariaConfigService service,
            IToastNotification toastNotification,
            IMascotaService mascotaService,
            IClienteService clienteService,
            IVeterinarioService veterinarioService,
            ITurnoService turnoService,
            IAtencionVeterinariaService atencionVeterinariaService,
            IPermissionService permissionService,
            RoleManager<Rol> roleManager,
            UserManager<Usuario> userManager
            )
        {
            _mascotaService = mascotaService;
            _veterinarioService = veterinarioService;
            _clienteService = clienteService;
            _veterinariaConfigService = service;
            _toastNotification = toastNotification;
            _turnoService = turnoService;
            _atencionVeterinariaService = atencionVeterinariaService;
            _permissionService = permissionService;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        #region PAGINA PRINCIPAL
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal(
            string busquedaVeterinario = null,
            string busquedaCliente = null,
            string busquedaMascota = null,
            int page = 1,
            int pageMascota = 1,
            int pageVet = 1)
        {
            var viewModel = new VeterinariaPaginaPrincipalViewModel();
            int pageSizeClientes = 3;
            int pageSizeMascotas = 3;
            int pageSizeVeterinarios = 3;


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



            // Preparamos datos para reportes dashboard

            viewModel.CantidadCitasHoy = await _turnoService.ContarTurnosParaFechaAsync(DateTime.Today);
            viewModel.CantidadClientesActivos = await _clienteService.ContarTotalClientesAsync();
            viewModel.CantidadMascotasRegistradas = await _mascotaService.ContarTotalMascotasAsync();
            viewModel.IngresosMensuales = await _atencionVeterinariaService.SumarCostosAtencionesMesActualAsync();
            viewModel.CantidadPerrosPeligrosos = await _mascotaService.ContarPerrosPeligrososAsync();

            var (vacunaNombre, _) = await _atencionVeterinariaService.ObtenerVacunaMasFrecuenteAsync(); 
            viewModel.VacunaMasFrecuenteNombre = vacunaNombre ?? "N/A";

            var (estudioNombre, estudioPrecio) = await _atencionVeterinariaService.ObtenerEstudioMasSolicitadoAsync();
            viewModel.EstudioMasSolicitadoNombre = estudioNombre ?? "N/A";
            viewModel.PrecioEstudioMasSolicitado = estudioPrecio;

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

        #region PERMISOS POR ROL

        [HttpGet]
        [Authorize(Policy = Permission.Administration.ManageRolePermissions)]
        public async Task<IActionResult> GestionPermissions(string SelectedRoleId)
        {
            var model = new ManagePermissionsPageViewModel();

            //  Obtener todos los roles 
            var allRoles = await _roleManager.Roles.ToListAsync();
            model.RolesList = allRoles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id.ToString()
            }).ToList();

            // cargar sus Permissions
            if (!string.IsNullOrEmpty(SelectedRoleId))
            {
                model.SelectedRoleId = SelectedRoleId;
                model.PermissionsForm = await _permissionService.GetRolePermissionsAsync(SelectedRoleId);
            }

            return View("GestionPermissions", model);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permission.Administration.ManageRolePermissions)]
        public async Task<IActionResult> GestionPermissions(RolPermissionsViewModel PermissionsForm)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Error de validacion");
                return RedirectToAction("GestionPermissions", new { roleId = PermissionsForm.RoleId });
            }

            try
            {
                bool success = await _permissionService.UpdateRolePermissionsAsync(PermissionsForm);

                if (success)
                {
                    _toastNotification.AddSuccessToastMessage("Permissions actualizados correctamente.");
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("Error al actualizar los Permissions.");
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error grave: {ex.Message}");
            }

         
            return RedirectToAction("GestionPermissions", new { roleId = PermissionsForm.RoleId });
        }
        #endregion

        #region PERMISOS USUARIO

        [HttpGet]
        [Authorize(Policy = Permission.Administration.ManageUsers)] 
        public async Task<IActionResult> GestionPermissionsUsuario(string SelectedUserId)
        {
            var model = new ManageUserPermissionsPageViewModel();

            // Obtener todos los usuarios 
            var allUsers = await _userManager.Users.ToListAsync();
            model.UsersList = allUsers.Select(u => new SelectListItem
            {
                Text = u.UserName, 
                Value = u.Id.ToString()
            }).ToList();

            //  cargar sus permisos
            if (!string.IsNullOrEmpty(SelectedUserId))
            {
                model.SelectedUserId = SelectedUserId;
                var userPermissions = await _permissionService.GetUserPermissionsAsync(SelectedUserId);
                if (userPermissions != null)
                {
                    model.PermissionsForm = userPermissions;
                }
            }

           
            return View("GestionPermissionsUsuario", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permission.Administration.ManageUsers)] 
        public async Task<IActionResult> GestionPermissionsUsuario(UserPermissionsViewModel PermissionsForm)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Error de validacion");

                return RedirectToAction("GestionPermissionsUsuario", new { SelectedUserId = PermissionsForm.UserId });
            }

            try
            {
                bool success = await _permissionService.UpdateUserPermissionsAsync(PermissionsForm);

                if (success)
                {
                    _toastNotification.AddSuccessToastMessage("Permisos de usuario actualizados correctamente");
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("Error al actualizar los permisos del usuario");
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error grave: {ex.Message}");
            }

            return RedirectToAction("GestionPermissionsUsuario", new { SelectedUserId = PermissionsForm.UserId });
        }


        #endregion

        #region ROLES DE USUARIO

        [HttpGet]
        [Authorize(Policy = Permission.Administration.ManageUsers)] 
        public async Task<IActionResult> GestionRolesUsuario(string SelectedUserId)
        {
            var model = new ManageUserRolesPageViewModel(); 

            // Obtener todos los usuarios 
            var allUsers = await _userManager.Users.ToListAsync();
            model.UsersList = allUsers.Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.Id.ToString()
            }).ToList();

            // cargar sus roles
            if (!string.IsNullOrEmpty(SelectedUserId))
            {
                model.SelectedUserId = SelectedUserId;
                var userRoles = await _permissionService.GetUserRolesAsync(SelectedUserId);
                if (userRoles != null)
                {
                    model.RolesForm = userRoles;
                }
            }

            return View("GestionRolesUsuario", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permission.Administration.ManageUsers)]
        public async Task<IActionResult> GestionRolesUsuario(UserRolesViewModel RolesForm)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Error de validacion");
                return RedirectToAction("GestionRolesUsuario", new { SelectedUserId = RolesForm.UserId });
            }

            try
            {
                bool success = await _permissionService.UpdateUserRolesAsync(RolesForm);
                if (success)
                {
                    _toastNotification.AddSuccessToastMessage("Roles del usuario actualizados correctamente.");
                }
               
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error grave: {ex.Message}");
            }

            return RedirectToAction("GestionRolesUsuario", new { SelectedUserId = RolesForm.UserId });
        }

        #endregion
    }
}
