using X.PagedList;

namespace SistemaVetIng.ViewsModels
{
    public class ClientePaginaPrincipalViewModel
    {
        public string NombreCompleto { get; set; }
        public List<MascotaListViewModel> Mascotas { get; set; }
        public ClienteViewModel Cliente { get; set; }
        public List<TurnoViewModel> Turnos { get; set; }
        public List<AtencionDetalleViewModel> PagosPendientes { get; set; }
        public IPagedList PaginacionTurnos { get; set; }
        public string BusquedaTurnoActual { get; set; }


        // Reportes Dashboard
        public int CantidadTurnos { get; set; }
        public int CantidadTurnosPendientesPorCliente { get; set; }
        public int CantidadMascotasPorCliente { get; set; }
        public int CantidadPagosPendientes { get; set; }

        public HistoriaClinicaViewModel HistoriaClinicas { get; set; } 

        public ClientePaginaPrincipalViewModel()
        {
            Mascotas = new List<MascotaListViewModel>();
            Cliente = new ClienteViewModel();
            Turnos = new List<TurnoViewModel>();
            HistoriaClinicas = new HistoriaClinicaViewModel();
            PagosPendientes = new List<AtencionDetalleViewModel>();
        }
    }
}
