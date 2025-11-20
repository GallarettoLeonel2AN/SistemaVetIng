using SistemaVetIng.Models;
using X.PagedList;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IClienteRepository
    {
        Task Agregar(Cliente entity);

        void Eliminar(Cliente entity);

        Task Guardar();

        Task<IEnumerable<Cliente>> ListarTodo();

        void Modificar(Cliente entity);

        Task<Cliente> ObtenerPorId(int id);

        Task<Cliente> ObtenerPorIdUsuario(int UsuarioId);

        Task<IPagedList<Cliente>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null);

        Task<int> ContarTotalClientesAsync();

        Task<bool> ExisteDniAsync(long dni);

        Task<List<Cliente>> GetClientesPorBusqueda(string busqueda);

        Task<Cliente> GetMascotasClientes(int clienteId);
    }
}
