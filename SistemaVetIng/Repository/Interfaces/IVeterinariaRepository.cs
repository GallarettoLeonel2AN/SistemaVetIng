using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IVeterinariaRepository
    {
        Task<Veterinaria> ObtenerPrimeraAsync();
        Task Guardar();
    }
}
