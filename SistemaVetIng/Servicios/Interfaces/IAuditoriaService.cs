using SistemaVetIng.Models;
using X.PagedList;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IAuditoriaService
    {
        Task RegistrarEventoAsync(int usuarioId, string nombreUsuario, string tipoEvento, string entidad,string? detalles = null);
        Task<IPagedList<AuditoriaEvento>> ObtenerLogPaginadoAsync(int pageNumber, int pageSize, string? busquedaUsuario = null, string? tipoEvento = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    }
}
