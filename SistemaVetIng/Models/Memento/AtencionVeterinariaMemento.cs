namespace SistemaVetIng.Models.Memento
{
    public class AtencionVeterinariaMemento
    {
        public int Id { get; set; }

        // Relación con la atención original (Para saber de quién es esta copia)
        public int AtencionVeterinariaId { get; set; }

        // Metadatos de la versión
        public DateTime FechaVersion { get; set; } = DateTime.Now;
        public string UsuarioEditor { get; set; } 
        public string? MotivoCambio { get; set; } // Ej: "Corregir diagnóstico"

        // Datos historicos
        public string Diagnostico { get; set; }
        public float PesoMascota { get; set; }

        public string? TratamientoMedicamento { get; set; }
        public string? TratamientoDosis { get; set; }
        public string? TratamientoFrecuencia { get; set; }
        public string? TratamientoDuracion { get; set; }
        public string? TratamientoObservaciones { get; set; }

        // vacunas y estudios
        public string? VacunasSnapshot { get; set; } 
        public string? EstudiosSnapshot { get; set; }
    }
}
