using SistemaVetIng.Models;
using SistemaVetIng.Models.Singleton;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class VeterinariaService : IVeterinariaService
    {
        private readonly IVeterinarioService _veterinarioService;
        private readonly IClienteService _clienteService;
        private readonly IMascotaService _mascotaService;
        private readonly IVeterinariaConfigService _veterinariaConfigService;
        private readonly IConfiguracionHorarioCache _cache;

        public VeterinariaService(
            IVeterinarioService veterinarioService,
            IClienteService clienteService,
            IMascotaService mascotaService,
            IConfiguracionHorarioCache cache,
            IVeterinariaConfigService veterinariaConfigService)
        {
            _veterinarioService = veterinarioService;
            _clienteService = clienteService;
            _mascotaService = mascotaService;
            _cache = cache;
            _veterinariaConfigService = veterinariaConfigService;
        }

        public async Task<VeterinariaPaginaPrincipalViewModel> PaginaPrincipalAsync(
                     string busquedaVeterinario = null,
                     string busquedaCliente = null,
                     string busquedaMascota = null)
        {
            var viewModel = new VeterinariaPaginaPrincipalViewModel();


            // Intentamos obtener config desde cache
            if (_cache.Configuracion == null)
            {
                var configDb = await _veterinariaConfigService.ObtenerConfiguracionAsync();

                if (configDb != null)
                    _cache.SetConfiguracion(configDb);
            }

            var configuracion = _cache.Configuracion;

            // Mapear confg solo si existe
            if (configuracion != null)
            {
                viewModel.ConfiguracionTurnos = new ConfiguracionVeterinariaViewModel
                {
                    Id = configuracion.Id,
                    DuracionMinutosPorConsulta = configuracion.DuracionMinutosPorConsulta,
                    Horarios = configuracion.HorariosPorDia.Select(h => new HorarioDiaViewModel
                    {
                        DiaSemana = h.DiaSemana,
                        EstaActivo = h.EstaActivo,
                        HoraInicio = h.HoraInicio ?? DateTime.MinValue,
                        HoraFin = h.HoraFin ?? DateTime.MinValue
                    })
                    .OrderBy(h => h.DiaSemana)
                    .ToList()
                };
            }
            else
            {
                viewModel.ConfiguracionTurnos = null;
            }

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

            var clientes = string.IsNullOrWhiteSpace(busquedaCliente)
                ? await _clienteService.ListarTodo()
                : await _clienteService.FiltrarPorBusqueda(busquedaCliente);

            viewModel.Clientes = clientes.Select(c => new ClienteViewModel
            {
                Id = c.Id,
                NombreCompleto = $"{c.Nombre} {c.Apellido}",
                Telefono = c.Telefono,
                NombreUsuario = c.Usuario?.Email,
                DNI = c.Dni 
            }).ToList();

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

            return viewModel;
        }
    }
}
