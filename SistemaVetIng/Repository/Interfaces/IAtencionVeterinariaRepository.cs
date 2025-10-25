using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IAtencionVeterinariaRepository
    {
        Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId);
        Task<List<Vacuna>> GetVacunas();
        Task<List<Estudio>> GetEstudios();
        Task<Veterinario> GetVeterinarioPorId(int usuarioId);
        Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids);
        Task<List<Estudio>> GetEstudioSeleccionado(IEnumerable<int> ids);
        Task AgregarAtencionVeterinaria(AtencionVeterinaria atencion);
        Task AgregarTratamiento(Tratamiento tratamiento);
        Task<List<AtencionVeterinaria>> ObtenerAtencionesPendientesPorCliente(int clienteId);
        Task<AtencionVeterinaria> ObtenerAtencionConCliente(int idAtencion);
        Task<AtencionVeterinaria> ObtenerPorId(int id);
        Task SaveChangesAsync();
        Task<HistoriaClinica> GetHistoriaClinicaPorId(int id);

        Task<decimal> SumarCostosAtencionesMesActualAsync();
        Task<(string Nombre, string Lote)> ObtenerVacunaMasFrecuenteAsync();
        Task<(string Nombre, decimal Precio)> ObtenerEstudioMasSolicitadoAsync();
        Task<int> CantidadAtencionesPorVeterinario(int id);
        Task<Mascota> ObtenerMascotaMasFrecuentePorVeterinario(int idVeterinario);
        Task<int> CantidadPagosPendientes(int idCliente);
    }
}
