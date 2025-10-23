using X.PagedList;

namespace SistemaVetIng.ViewsModels
{
    public class VeterinarioPaginaPrincipalViewModel
    {
        public List<ClienteViewModel> Clientes { get; set; }
        public List<MascotaListViewModel> Mascotas { get; set; }
        public List<TurnoViewModel> CitasDeHoy { get; set; } = new List<TurnoViewModel>();
        public IPagedList PaginacionClientes { get; set; }
        public IPagedList PaginacionMascotas { get; set; }
        public VeterinarioPaginaPrincipalViewModel()
        {
            Clientes = new List<ClienteViewModel>();
            Mascotas = new List<MascotaListViewModel>();
            CitasDeHoy = new List<TurnoViewModel>();
        }
    }
}
