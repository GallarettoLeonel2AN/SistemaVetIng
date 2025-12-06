using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IChipRepository
    {
        Task<IEnumerable<Chip>> ListarTodo();
        Task<Chip> ObtenerPorId(int id);
        Task Agregar(Chip entity);
        void Modificar(Chip entity);
        void Eliminar(Chip entity);
        Task Guardar();
        Task<bool> PoseeChipMascota(int mascotaId);
        Task<Chip> ObtenerPorMascotaId(int mascotaId);
    }
}
