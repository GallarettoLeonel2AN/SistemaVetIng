using PerrosPeligrososApi.Models;
using PerrosPeligrososApi.Models.Dtos;

namespace PerrosPeligrososApi.Services.Interface
{
    public interface IPerroPeligrosoService
    {
        Task<int> ProcesarRegistro(PerroPeligrosoRegistroDto registroDto);
        Task<IEnumerable<PerroPeligrosoResponseDto>> ObtenerTodos();
        Task<PerroPeligrosoResponseDto> ObtenerPorId(int id);
        Task<IEnumerable<PerroPeligrosoResponseDto>> Buscar(string termino);
    }
}
