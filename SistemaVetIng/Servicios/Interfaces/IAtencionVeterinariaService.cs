using SistemaVetIng.Models;
using SistemaVetIng.Models.Memento;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IAtencionVeterinariaService
    {
        Task<AtencionVeterinaria> ObtenerPorId(int id);

        Task<List<AtencionDetalleViewModel>> ObtenerPagosPendientesPorClienteId(int clienteId);
        Task<AtencionVeterinariaViewModel> GetAtencionVeterinariaViewModel(int historiaClinicaId);
        Task<string> CreateAtencionVeterinaria(AtencionVeterinariaViewModel model, ClaimsPrincipal user);
        Task RegistrarAtencionDesdeTurnoAsync(AtencionPorTurnoViewModel model, ClaimsPrincipal user);
        Task<decimal> SumarCostosAtencionesMesActualAsync();
        Task<(string Nombre, string Lote)> ObtenerVacunaMasFrecuenteAsync();
        Task<(string Nombre, decimal Precio)> ObtenerEstudioMasSolicitadoAsync();
        Task<int> CantidadAtencionesPorVeterinario(int id);
        Task<Mascota> ObtenerMascotaMasFrecuentePorVeterinario(int idVeterinario);
        Task<int> CantidadPagosPendientes(int idCliente);
        Task<int> CantidadAtenciones();
        Task<Cliente> ObtenerClienteMasFrecuenteAsync();
        Task<decimal> SumarIngresosAsync();
        Task<List<DashboardViewModel.IngresosAnualesData>> ObtenerDatosIngresosAnualesAsync(List<int> anios);
        Task<List<DashboardViewModel.ServicioCountData>> ContarTopServiciosAsync(int topN);
        Task<List<DashboardViewModel.IngresosMensualesData>> ObtenerDatosIngresosMensualesAsync(int anio);
        Task<List<DashboardViewModel.AtencionesPorVeterinarioData>> ContarAtencionesPorVeterinarioAsync(DateTime? inicio, DateTime? fin);
        Task<List<AtencionVeterinaria>> ObtenerAtencionesPorMesAsync(int anio, int mes);
        Task<List<AtencionVeterinaria>> ObtenerAtencionesPorIdCliente(List<int> ids);
        Task ActualizarAtencionesAsync(List<AtencionVeterinaria> atenciones);
        Task ActualizarAtencionAsync(AtencionVeterinaria atencion);

        #region Memento
        Task<AtencionVeterinariaViewModel> ObtenerAtencionParaEditarAsync(int id);
        Task EditarAtencionConRespaldoAsync(AtencionVeterinariaViewModel model, ClaimsPrincipal user, string motivo);
        Task RestaurarVersionAsync(int mementoId);
        Task<List<AtencionVeterinariaMemento>> ObtenerHistorialAsync(int atencionId);
        #endregion

    }
}
