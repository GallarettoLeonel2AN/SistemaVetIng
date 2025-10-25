using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;
using X.PagedList;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IVeterinarioService
    {
        Task<Veterinario> Registrar(VeterinarioRegistroViewModel viewModel);
        Task<Veterinario> Modificar(VeterinarioEditarViewModel viewModel);
        Task Eliminar(int id);
        Task<Veterinario> ObtenerPorId(int id);
        Task<Veterinario> ObtenerPorIdUsuario(int id);
        Task<IEnumerable<Veterinario>> ListarTodo();
        Task<IEnumerable<Veterinario>> FiltrarPorBusqueda(string busqueda);
        Task<IPagedList<Veterinario>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null);
    }
}
