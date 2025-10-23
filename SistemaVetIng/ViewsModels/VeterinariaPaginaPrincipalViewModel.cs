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

        // Propiedades para los reportes analíticos simulados
        public int CantidadPerrosPeligrosos { get; set; }
        public string RazaMayorDemanda { get; set; } 
        public decimal IngresosMensualesEstimados { get; set; } 
        public decimal IngresosDiariosEstimados { get; set; } 

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
