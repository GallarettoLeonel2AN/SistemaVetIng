using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IVacunaRepository : IGeneralRepository<Vacuna>
    {

     
        Task<Vacuna> ObtenerPorIdAsync(int id);

        Task<IEnumerable<Vacuna>> ObtenerPorIdsAsync(List<int> ids);
    }
}
