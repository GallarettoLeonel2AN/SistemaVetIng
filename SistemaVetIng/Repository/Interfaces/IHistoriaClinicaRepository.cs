using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IHistoriaClinicaRepository
    {
        Task<Mascota> GetHistoriaClinicaCompletaMascota(int mascotaId);
        Task<HistoriaClinica> ObtenerPorMascotaIdAsync(int mascotaId);
        Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId);
        Task<HistoriaClinica> GetHistoriaClinicaPorId(int id);
    }
}
