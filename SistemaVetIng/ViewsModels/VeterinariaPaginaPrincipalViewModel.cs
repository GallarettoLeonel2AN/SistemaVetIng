using SistemaVetIng.Models;
using X.PagedList;

namespace SistemaVetIng.ViewsModels
{
    public class VeterinariaPaginaPrincipalViewModel
    {
        public List<VeterinarioViewModel> Veterinarios { get; set; }
        public IPagedList PaginacionClientes { get; set; }
        public IPagedList PaginacionMascotas { get; set; }
        public IPagedList PaginacionVeterinarios { get; set; }
        public ConfiguracionVeterinariaViewModel ConfiguracionTurnos { get; set; }
        public List<ClienteViewModel> Clientes { get; set; } 
        public List<MascotaListViewModel> Mascotas { get; set; }
        public List<TurnoViewModel> CitasDeHoy { get; set; } = new List<TurnoViewModel>();

        // Propiedades para Cards de Reportes
        public int CantidadCitasHoy { get; set; }
        public int CantidadClientesActivos { get; set; } 
        public int CantidadMascotasRegistradas { get; set; }
        public decimal IngresosMensuales { get; set; } 
        public int CantidadPerrosPeligrosos { get; set; }
        public string VacunaMasFrecuenteNombre { get; set; }
        public string EstudioMasSolicitadoNombre { get; set; }
        public decimal? PrecioEstudioMasSolicitado { get; set; } // Nullable por si no hay estudios

        public VeterinariaPaginaPrincipalViewModel()
        {
            Veterinarios = new List<VeterinarioViewModel>();
            Clientes = new List<ClienteViewModel>();
            Mascotas = new List<MascotaListViewModel>();
            ConfiguracionTurnos = new ConfiguracionVeterinariaViewModel();
            CitasDeHoy = new List<TurnoViewModel>();
        }
    }
}
