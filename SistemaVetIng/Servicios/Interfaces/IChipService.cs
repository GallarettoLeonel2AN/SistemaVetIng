namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IChipService
    {
        Task<bool> PoseeChipMascota(int mascotaId);
    }
}
