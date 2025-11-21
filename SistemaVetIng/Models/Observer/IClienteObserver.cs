namespace SistemaVetIng.Models.Observer
{
    public interface IClienteObserver
    {
        Task NotificarClienteRegistradoAsync(Cliente cliente);
    }
}
