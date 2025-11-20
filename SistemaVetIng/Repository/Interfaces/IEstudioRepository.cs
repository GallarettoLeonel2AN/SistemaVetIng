using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IEstudioRepository 
    {
        Task Agregar(Estudio entity);
        Task Guardar();
        Task<IEnumerable<Estudio>> ListarTodo();
        void Modificar(Estudio entity);
        void Eliminar(Estudio entity);
        Task<Estudio> ObtenerPorId(int id);
        Task<Estudio> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Estudio>> ObtenerPorIdsAsync(List<int> ids);
        Task<List<Estudio>> GetEstudioSeleccionado(IEnumerable<int> ids);

    }
}
