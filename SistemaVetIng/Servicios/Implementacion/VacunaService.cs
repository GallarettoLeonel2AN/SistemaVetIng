using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using X.PagedList;
using X.PagedList.Extensions;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class VacunaService : IVacunaService
    {
        private readonly IVacunaRepository _vacunaRepository;

        public VacunaService(IVacunaRepository vacunaRepository)
        {
            _vacunaRepository = vacunaRepository;
        }

        private Vacuna MapToEntity(VacunaViewModel model) => new Vacuna
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Lote = model.Lote,
            Precio = model.Precio
        };

        // Función de mapeo de Entidad a ViewModel
        private VacunaViewModel MapToViewModel(Vacuna entity) => new VacunaViewModel
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Lote = entity.Lote,
            Precio = entity.Precio
        };

        public async Task<(bool success, string message)> Registrar(VacunaViewModel model)
        {
            try
            {
                var vacuna = MapToEntity(model);
                await _vacunaRepository.Agregar(vacuna);
                await _vacunaRepository.Guardar();
                return (true, $"Vacuna '{vacuna.Nombre}' (Lote: {vacuna.Lote}) registrada exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar la vacuna: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> Modificar(VacunaViewModel model)
        {
            var vacuna = await _vacunaRepository.ObtenerPorId(model.Id);
            if (vacuna == null) return (false, "Vacuna no encontrada.");

            try
            {
                // Actualizar los campos
                vacuna.Nombre = model.Nombre;
                vacuna.Lote = model.Lote;
                vacuna.Precio = model.Precio;

                _vacunaRepository.Modificar(vacuna);
                await _vacunaRepository.Guardar();
                return (true, $"Vacuna '{vacuna.Nombre}' (Lote: {vacuna.Lote}) modificada exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al modificar la vacuna: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> Eliminar(int id)
        {
            var vacuna = await _vacunaRepository.ObtenerPorId(id);
            if (vacuna == null) return (false, "Vacuna no encontrada para eliminar.");

            try
            {
                _vacunaRepository.Eliminar(vacuna);
                await _vacunaRepository.Guardar();
                return (true, $"Vacuna '{vacuna.Nombre}' (Lote: {vacuna.Lote}) eliminada exitosamente.");
            }
            catch (Exception ex)
            {
               
                return (false, $"Error al eliminar: Es posible que esta vacuna esté asociada a atenciones veterinarias. {ex.Message}");
            }
        }

        public async Task<VacunaViewModel?> ObtenerPorId(int id)
        {
            var vacuna = await _vacunaRepository.ObtenerPorId(id);
            return vacuna != null ? MapToViewModel(vacuna) : null;
        }

        public async Task<IEnumerable<VacunaViewModel>> ListarTodo()
        {
            var vacunas = await _vacunaRepository.ListarTodo();
            return vacunas.Select(MapToViewModel);
        }

        public async Task<IPagedList<VacunaViewModel>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null)
        {
            
            var vacunas = await _vacunaRepository.ListarTodo();

            //  Convertir a IQueryable para facilitar la paginación en memoria (usando AsQueryable)
            var query = vacunas.AsQueryable();

            // Aplicar filtrado si existe la búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var busquedaLower = busqueda.ToLower();
                query = query.Where(v => v.Nombre.ToLower().Contains(busquedaLower) ||
                                         v.Lote.ToLower().Contains(busquedaLower));
            }

          
            query = query.OrderBy(v => v.Nombre).ThenBy(v => v.Lote);

            // paginación (X.PagedList funciona con IQueryable en memoria)
            var paginatedEntities = query.ToPagedList(pageNumber, pageSize);

            // Mapear al ViewModel
            var viewModels = paginatedEntities.Select(MapToViewModel).ToList();

            // Retornar el StaticPagedList
            return new StaticPagedList<VacunaViewModel>(
                viewModels,
                paginatedEntities.GetMetaData()
            );
        }

        public async Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids)
        {
            return await _vacunaRepository.GetVacunaSeleccionada(ids);
        }

        public async Task<IEnumerable<Vacuna>> ObtenerPorIdsAsync(List<int> ids)
        {
            return await _vacunaRepository.ObtenerPorIdsAsync(ids);
        }
    }
}
