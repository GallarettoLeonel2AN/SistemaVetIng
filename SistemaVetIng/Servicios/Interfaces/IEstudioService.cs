using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;
using X.PagedList;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IEstudioService
    {
        Task<(bool success, string message)> Registrar(EstudioViewModel model);
        Task<(bool success, string message)> Modificar(EstudioViewModel model);
        Task<(bool success, string message)> Eliminar(int id);
        Task<EstudioViewModel?> ObtenerPorId(int id);
        Task<IEnumerable<EstudioViewModel>> ListarTodo();
        // Usaremos esta interfaz para la paginación, implementando la lógica en el servicio
        Task<IPagedList<EstudioViewModel>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null);
      
        Task<IEnumerable<Estudio>> ObtenerPorIdsAsync(List<int> ids);
    }
}
