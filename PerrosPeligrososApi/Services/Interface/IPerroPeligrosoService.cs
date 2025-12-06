using PerrosPeligrososApi.Models;

namespace PerrosPeligrososApi.Services.Interface
{
    public interface IPerroPeligrosoService
    {
        Task<int> ProcesarRegistro(PerroPeligrosoRegistroDto registroDto);
    }
}
