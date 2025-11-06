using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;
using X.PagedList;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IMascotaService
    {
        Task<(Mascota? mascota, bool success, string message)> Registrar(MascotaRegistroViewModel model,int auditUserId,string auditUserName,string rolUsuario);
        Task<(bool success, string message)> Modificar(MascotaEditarViewModel viewModel);
        Task<(bool success, string message)> Eliminar(int id);
        Task<Mascota> ObtenerPorId(int id);
        Task<IEnumerable<Mascota>> ListarTodo();
        Task<IEnumerable<Mascota>> FiltrarPorBusqueda(string busqueda);
        Task<IEnumerable<Mascota>> ListarMascotasPorClienteId(int clienteId);
        Task<IEnumerable<MascotaListViewModel>> ObtenerMascotasPorClienteUserNameAsync(string userName);
        Task<MascotaDetalleViewModel> ObtenerDetalleConHistorial(int mascotaId);
        Task<IPagedList<Mascota>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null);
        Task<int> ContarTotalMascotasAsync();
        Task<int> ContarPerrosPeligrososAsync();
        Task<int> ContarTotalMascotasPorClienteAsync(int idCliente);
        Task<List<DashboardViewModel.EspecieCountData>> ContarMascotasPorEspecieAsync();

        Task<int> ContarPerrosChipAsync();
        Task<List<DashboardViewModel.RazaData>> ObtenerRazasPorEspecieAsync(string especie);

    }
}
