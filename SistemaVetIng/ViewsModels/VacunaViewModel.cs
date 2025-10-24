using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class VacunaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nombre de Vacuna")]
        [Required(ErrorMessage = "El nombre de la vacuna es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; }

        [Display(Name = "Lote / Identificador")]
        [Required(ErrorMessage = "El número de lote es obligatorio.")]
        [StringLength(50, ErrorMessage = "El lote no puede exceder los 50 caracteres.")]
        public string Lote { get; set; }

        [Display(Name = "Precio de Venta")]
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [DataType(DataType.Currency)]
        [Range(0.01, 1000000.00, ErrorMessage = "El precio debe ser un valor positivo.")]
        public decimal Precio { get; set; }
    }
}