using SistemaVetIng.Models;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IHistoriaClinicaService
    {
        Task<List<Cliente>> GetClientesParaSeguimiento(string searchString);
        Task<Cliente> GetMascotasCliente(int clienteId);
        Task<Mascota> GetDetalleHistoriaClinica(int mascotaId);
        Task<HistoriaClinica> ObtenerPorMascotaIdAsync(int mascotaId);
        Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId);
        Task<HistoriaClinica> GetHistoriaClinicaPorId(int id);
    }
}
