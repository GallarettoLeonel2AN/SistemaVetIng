namespace SistemaVetIng.Models.Observer
{
    public interface IClienteSubject
    {
        void AgregarObserver(IClienteObserver observer);
        Task NotificarAsync(Cliente cliente);
    }
}
