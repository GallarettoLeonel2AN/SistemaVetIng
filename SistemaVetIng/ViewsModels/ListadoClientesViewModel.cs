using SistemaVetIng.Models;
using X.PagedList;

namespace SistemaVetIng.ViewsModels
{
    public class ListadoClientesViewModel
    {
        public IPagedList<Cliente> ClientesPaginados { get; set; }
        public string BusquedaActual { get; set; }
    }
}
