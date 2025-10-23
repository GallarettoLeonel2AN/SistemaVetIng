using SistemaVetIng.Models;
using X.PagedList;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IVeterinarioRepository
    {
        Task<IEnumerable<Veterinario>> ListarTodo();
        Task<Veterinario> ObtenerPorId(int id);
        Task Agregar(Veterinario entity);
        void Modificar(Veterinario entity);
        void Eliminar(Veterinario entity);
        Task Guardar();
        Task<IPagedList<Veterinario>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null);
    }
}
