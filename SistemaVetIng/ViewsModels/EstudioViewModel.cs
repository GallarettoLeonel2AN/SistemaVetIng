using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class EstudioViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nombre del Estudio")]
        [Required(ErrorMessage = "El nombre del estudio es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; }


        [Display(Name = "Precio de Venta")]
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [DataType(DataType.Currency)]
        [Range(0.01, 1000000.00, ErrorMessage = "El precio debe ser un valor positivo.")]
        public decimal Precio { get; set; }

        public string? Informe { get; set; }
    }
}