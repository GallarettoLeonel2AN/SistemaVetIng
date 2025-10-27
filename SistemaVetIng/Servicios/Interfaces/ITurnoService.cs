using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;
using X.PagedList;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface ITurnoService
    {
        Task<List<string>> GetHorariosDisponiblesAsync(DateTime fecha);
        Task ReservarTurnoAsync(ReservaTurnoViewModel model);
        Task<IEnumerable<Turno>> ObtenerTurnosAsync();
        Task<IEnumerable<Turno>> ObtenerTurnosPorClienteIdAsync(int clienteId);
        Task<IEnumerable<Turno>> ObtenerTurnosPorFechaAsync(DateTime fecha);
        Task<Turno> ObtenerPorIdConDatosAsync(int id);
        void Actualizar(Turno turno);
        Task Guardar();
        Task<(bool success, string message)> CancelarTurnoAsync(int turnoId, ClaimsPrincipal user);
        Task<IPagedList<Turno>> ListarPaginadoPorClienteAsync(int clienteId, int pageNumber, int pageSize, string busqueda = null);
        Task<int> ContarTurnosParaFechaAsync(DateTime fecha);
        Task<int> ContarTotalTurnosClienteAsync(int idCliente);
        Task<int> CantidadTurnosPendientesPorCliente(int idCliente);
        Task<double> CalcularPorcentajeAusentismoAsync();
        Task<int> CantidadTurnosAsync();
        Task<int> ContarTurnosPorEstadoYFechaAsync(string estado, DateTime fecha);
    }
}
