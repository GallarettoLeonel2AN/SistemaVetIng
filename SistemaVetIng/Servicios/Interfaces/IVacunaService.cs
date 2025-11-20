using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;
using X.PagedList;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IVacunaService
    {
        Task<(bool success, string message)> Registrar(VacunaViewModel model);
        Task<(bool success, string message)> Modificar(VacunaViewModel model);
        Task<(bool success, string message)> Eliminar(int id);
        Task<VacunaViewModel?> ObtenerPorId(int id);
        Task<IEnumerable<VacunaViewModel>> ListarTodo();
        Task<IPagedList<VacunaViewModel>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null);

        Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids);
        Task<IEnumerable<Vacuna>> ObtenerPorIdsAsync(List<int> ids);
    }
}
