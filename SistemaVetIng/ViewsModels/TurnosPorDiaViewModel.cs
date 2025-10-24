using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class TurnosPorDiaViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Seleccionar Fecha")]
        public DateTime FechaSeleccionada { get; set; } = DateTime.Today; // Por defecto hoy

        public List<TurnoViewModel> TurnosDelDia { get; set; } = new List<TurnoViewModel>();
    }
}
