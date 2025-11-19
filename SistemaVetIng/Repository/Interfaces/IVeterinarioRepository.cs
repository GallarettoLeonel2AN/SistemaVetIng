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
        Task<Veterinario> ObtenerPorIdUsuario(int Usuario);
        void Eliminar(Veterinario entity);
        Task Guardar();
        Task<IPagedList<Veterinario>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null);
        Task<bool> ExisteDniAsync(long dni);
    }
}
