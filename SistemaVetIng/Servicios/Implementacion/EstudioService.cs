using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using X.PagedList.Extensions;
using X.PagedList;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class EstudioService : IEstudioService
    {
        private readonly IEstudioRepository _estudioRepository;

        public EstudioService(IEstudioRepository estudioRepository)
        {
            _estudioRepository = estudioRepository;
        }

        private Estudio MapToEntity(EstudioViewModel model) => new Estudio
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Precio = model.Precio,
           
        };

        private EstudioViewModel MapToViewModel(Estudio entity) => new EstudioViewModel
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Precio = entity.Precio,
           
        };

        public async Task<(bool success, string message)> Registrar(EstudioViewModel model)
        {
            try
            {
                var estudio = MapToEntity(model);
                await _estudioRepository.Agregar(estudio);
                await _estudioRepository.Guardar();
                return (true, $"Estudio '{estudio.Nombre}' registrado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar el estudio: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> Modificar(EstudioViewModel model)
        {
            var estudio = await _estudioRepository.ObtenerPorId(model.Id);
            if (estudio == null) return (false, "Estudio no encontrado.");

            try
            {
                estudio.Nombre = model.Nombre;
                estudio.Precio = model.Precio;
             

                _estudioRepository.Modificar(estudio);
                await _estudioRepository.Guardar();
                return (true, $"Estudio '{estudio.Nombre}' modificado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al modificar el estudio: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> Eliminar(int id)
        {
            var estudio = await _estudioRepository.ObtenerPorId(id);
            if (estudio == null) return (false, "Estudio no encontrado para eliminar.");

            try
            {
                _estudioRepository.Eliminar(estudio);
                await _estudioRepository.Guardar();
                return (true, $"Estudio '{estudio.Nombre}' eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar: El estudio podría estar asociado a atenciones veterinarias. {ex.Message}");
            }
        }

        public async Task<EstudioViewModel?> ObtenerPorId(int id)
        {
            var estudio = await _estudioRepository.ObtenerPorId(id);
            return estudio != null ? MapToViewModel(estudio) : null;
        }

        public async Task<IEnumerable<EstudioViewModel>> ListarTodo()
        {
            var estudios = await _estudioRepository.ListarTodo();
            return estudios.Select(MapToViewModel);
        }

        public async Task<IPagedList<EstudioViewModel>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null)
        {
            var estudios = await _estudioRepository.ListarTodo();
            var query = estudios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var busquedaLower = busqueda.ToLower();
                query = query.Where(e => e.Nombre.ToLower().Contains(busquedaLower));
            }

            query = query.OrderBy(e => e.Nombre);

            var paginatedEntities = query.ToPagedList(pageNumber, pageSize);
            var viewModels = paginatedEntities.Select(MapToViewModel).ToList();

            return new StaticPagedList<EstudioViewModel>(
                viewModels,
                paginatedEntities.GetMetaData()
            );
        }
     
        public async Task<IEnumerable<Estudio>> ObtenerPorIdsAsync(List<int> ids)
        {
            return await _estudioRepository.ObtenerPorIdsAsync(ids);
        }

        public async Task<List<Estudio>> GetEstudioSeleccionado(IEnumerable<int> ids)
        {
            return await _estudioRepository.GetEstudioSeleccionado(ids);
        }
    }
}
