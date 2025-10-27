using SistemaVetIng.Models;
using X.PagedList;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface ITurnoRepository
    {
        Task<IEnumerable<Turno>> GetTurnosByFecha(DateTime fecha);
        Task AgregarTurno(Turno turno);
        Task Guardar();
        Task<IEnumerable<Turno>> ListarTodo();
        Task<IEnumerable<Turno>> ObtenerTurnosPorClienteIdAsync(int clienteId);
        Task<IEnumerable<Turno>> ObtenerTurnosPorFechaAsync(DateTime fecha);
        Task<Turno> ObtenerPorIdConDatosAsync(int id);
        void Actualizar(Turno turno);
        Task<IPagedList<Turno>> ListarPaginadoPorClienteAsync(int clienteId, int pageNumber, int pageSize, string busqueda = null);
        Task<int> ContarTurnosParaFechaAsync(DateTime fecha);
        Task<int> ContarTotalTurnosClienteAsync(int idCliente);
        Task<int> CantidadTurnosPendientesPorCliente(int idCliente);
        Task<int> ContarTurnosPorEstadoAsync(string estado);
        Task<int> CantidadTurnosAsync();

        Task<int> ContarTurnosPorEstadoYFechaAsync(string estado, DateTime fecha);
    }
}
