using X.PagedList;

namespace SistemaVetIng.ViewsModels
{
    public class HistorialPagosViewModel
    {
        public IPagedList<PagosItemViewModel> PagosPaginados { get; set; }
    }
}
