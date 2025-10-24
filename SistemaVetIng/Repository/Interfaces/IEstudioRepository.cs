using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IEstudioRepository : IGeneralRepository<Estudio>
    {
        
        Task<Estudio> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Estudio>> ObtenerPorIdsAsync(List<int> ids);
    }
}
