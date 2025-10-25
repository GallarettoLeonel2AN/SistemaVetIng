using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IAtencionVeterinariaService
    {
        Task<AtencionVeterinaria> ObtenerPorId(int id);

        Task<List<AtencionDetalleViewModel>> ObtenerPagosPendientesPorClienteId(int clienteId);
        Task<AtencionVeterinariaViewModel> GetAtencionVeterinariaViewModel(int historiaClinicaId);
        Task<string> CreateAtencionVeterinaria(AtencionVeterinariaViewModel model, ClaimsPrincipal user);
        Task RegistrarAtencionDesdeTurnoAsync(AtencionPorTurnoViewModel model, ClaimsPrincipal user);
        Task<decimal> SumarCostosAtencionesMesActualAsync();
        Task<(string Nombre, string Lote)> ObtenerVacunaMasFrecuenteAsync();
        Task<(string Nombre, decimal Precio)> ObtenerEstudioMasSolicitadoAsync();
        Task<int> CantidadAtencionesPorVeterinario(int id);
        Task<Mascota> ObtenerMascotaMasFrecuentePorVeterinario(int idVeterinario);
        Task<int> CantidadPagosPendientes(int idCliente);
    }
}
