using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaVetIng.ViewsModels
{
    public class AtencionVeterinariaViewModel
    {

        public int Id { get; set; }
        // Propiedades de AtencionVeterinaria
        [Display(Name = "Fecha de Atención")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El diagnóstico es obligatorio.")]
        [StringLength(500, ErrorMessage = "El diagnóstico no puede exceder los 500 caracteres.")]
        [Display(Name = "Diagnóstico")]
        public string? Diagnostico { get; set; }

        [Required(ErrorMessage = "El peso es obligatorio.")]
        [Range(0.1, 500.0, ErrorMessage = "El peso debe estar entre 0.1 y 500 kg.")]
        [Display(Name = "Peso (kg)")]
        public float PesoKg { get; set; }

        
        [Required]
        public int HistoriaClinicaId { get; set; }

        
        public int VeterinarioId { get; set; }

        public int MascotaId { get; set; }
        public string? MascotaNombre { get; set; }
        public string? PropietarioNombre { get; set; }

        // Propiedades para los SelectList de la vista
        public SelectList? VacunasDisponibles { get; set; }
        public SelectList? EstudiosDisponibles { get; set; }
        public object? VacunasConPrecio { get; set; }
        public object? EstudiosConPrecio { get; set; }

        [Required(ErrorMessage = "El medicamento es obligatorio.")]
        [StringLength(200, ErrorMessage = "El medicamento no puede exceder los 200 caracteres.")]
        [Display(Name = "Medicamento")]
        public string? Medicamento { get; set; }

        [Required(ErrorMessage = "La dosis es obligatoria.")]
        [StringLength(200, ErrorMessage = "La dosis no puede exceder los 200 caracteres.")]
        [Display(Name = "Dosis")]
        public string? Dosis { get; set; }

        [Required(ErrorMessage = "La frecuencia es obligatoria.")]
        [StringLength(200, ErrorMessage = "La frecuencia no puede exceder los 200 caracteres.")]
        [Display(Name = "Frecuencia")]
        public string? Frecuencia { get; set; }

        [Required(ErrorMessage = "La duración es obligatoria.")]
        [Range(0, 365, ErrorMessage = "La duración debe ser entre 0 y 365 días.")]
        [Display(Name = "Duración (días)")]
        public string DuracionDias { get; set; }

        [Required(ErrorMessage = "Las observaciones son obligatorias.")]
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres.")]
        [Display(Name = "Observaciones del Tratamiento")]
        public string? ObservacionesTratamiento { get; set; }

        
        public List<int> EstudiosSeleccionadosIds { get; set; } = new List<int>();
        public List<int> VacunasSeleccionadasIds { get; set; } = new List<int>();
    }
}