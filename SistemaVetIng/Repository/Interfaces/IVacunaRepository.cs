using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IVacunaRepository
    {
        Task Agregar(Vacuna entity);
        Task Guardar();
        Task<IEnumerable<Vacuna>> ListarTodo();
        void Modificar(Vacuna entity);
        void Eliminar(Vacuna entity);
        Task<Vacuna> ObtenerPorId(int id);
        Task<Vacuna> ObtenerPorIdAsync(int id);
        Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids);
        Task<IEnumerable<Vacuna>> ObtenerPorIdsAsync(List<int> ids);
    }
}
