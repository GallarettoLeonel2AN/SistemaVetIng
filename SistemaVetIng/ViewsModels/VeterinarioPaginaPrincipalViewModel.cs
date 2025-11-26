using SistemaVetIng.Models;
using X.PagedList;

namespace SistemaVetIng.ViewsModels
{
    public class VeterinarioPaginaPrincipalViewModel
    {
        public List<VeterinarioViewModel> Veterinarios { get; set; }
        public string Nombre { get; set; }
        public List<ClienteViewModel> Clientes { get; set; }
        public List<MascotaListViewModel> Mascotas { get; set; }
        public List<TurnoViewModel> CitasDeHoy { get; set; } 
        public ConfiguracionVeterinariaViewModel ConfiguracionTurnos { get; set; }
        public IPagedList PaginacionClientes { get; set; }
        public IPagedList PaginacionMascotas { get; set; }
        public IPagedList PaginacionVeterinarios { get; set; }
        public int CantidadCitasHoy { get; set; }
        public int CantidadAtencionesPorVeterinario{ get; set; }
        public Mascota MascotaMasFrecuentePorVeterinario { get; set; }
        public VeterinarioPaginaPrincipalViewModel()
        {
            Clientes = new List<ClienteViewModel>();
            Mascotas = new List<MascotaListViewModel>();
            CitasDeHoy = new List<TurnoViewModel>();
            ConfiguracionTurnos = new ConfiguracionVeterinariaViewModel();
            Veterinarios = new List<VeterinarioViewModel>();

        }
    }
}
