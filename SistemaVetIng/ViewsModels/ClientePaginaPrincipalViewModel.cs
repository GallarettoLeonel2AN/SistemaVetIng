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

        // Reportes simulados / indicadores

        // Reportes simulados / indicadores
        public int CantidadTurnosReservados { get; set; }
        public int CantidadTurnosCancelados { get; set; }
        public string VeterinarioMasFrecuente { get; set; }

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
