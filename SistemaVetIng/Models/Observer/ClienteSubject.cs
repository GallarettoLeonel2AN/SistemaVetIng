namespace SistemaVetIng.Models.Observer
{
    public class ClienteSubject : IClienteSubject
    {
        private readonly List<IClienteObserver> _observadores;

        public ClienteSubject(IEnumerable<IClienteObserver> observers)
        {
            _observadores = observers.ToList();
        }

        public void AgregarObserver(IClienteObserver observer)
        {
            _observadores.Add(observer);
        }

        public async Task NotificarAsync(Cliente cliente)
        {
            foreach (var obs in _observadores)
            {
                await obs.NotificarClienteRegistradoAsync(cliente);
            }
        }
    }
}
