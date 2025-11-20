using SistemaVetIng.Models;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class HistoriaClinicaService : IHistoriaClinicaService
    {
        private readonly IHistoriaClinicaRepository _repository;

        public HistoriaClinicaService(IHistoriaClinicaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Cliente>> GetClientesParaSeguimiento(string searchString)
        {
            return await _repository.GetClientesPorBusqueda(searchString);
        }

        public async Task<Cliente> GetMascotasCliente(int clienteId)
        {
            return await _repository.GetMascotasClientes(clienteId);
        }

        public async Task<Mascota> GetDetalleHistoriaClinica(int mascotaId)
        {
            return await _repository.GetHistoriaClinicaCompletaMascota(mascotaId);
        }

        public async Task<HistoriaClinica> ObtenerPorMascotaIdAsync(int mascotaId)
        {
            return await _repository.ObtenerPorMascotaIdAsync(mascotaId);
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId)
        {
            return await _repository.GetHistoriaClinicaConMascotayPropietario(historiaClinicaId);
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaPorId(int id)
        {
            return await _repository.GetHistoriaClinicaPorId(id);
        }
    }
}
