using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class ChipService : IChipService
    {

        private readonly IChipRepository _chipRepository;

        public ChipService(IChipRepository chipRepository)
        {
            _chipRepository = chipRepository;
        }

        public async Task<bool> PoseeChipMascota(int mascotaId)
          => await _chipRepository.PoseeChipMascota(mascotaId);
    }
}
