namespace SistemaVetIng.Models.Memento
{
    public class AtencionVeterinariaMemento
    {
        public int Id { get; set; }

        // Relación con la atención original (Para saber de quién es esta copia)
        public int AtencionVeterinariaId { get; set; }

        // Metadatos de la versión
        public DateTime FechaVersion { get; set; } = DateTime.Now;
        public string UsuarioEditor { get; set; } // Email o ID del usuario que editó
        public string? MotivoCambio { get; set; } // Ej: "Corregir diagnóstico"

        // --- DATOS HISTÓRICOS (La "Foto") ---
        public string Diagnostico { get; set; }
        public float PesoMascota { get; set; }

        // Aplanamos el Tratamiento aquí para no complicar la base de datos con FKs históricas
        public string? TratamientoMedicamento { get; set; }
        public string? TratamientoDosis { get; set; }
        public string? TratamientoFrecuencia { get; set; }
        public string? TratamientoDuracion { get; set; }
        public string? TratamientoObservaciones { get; set; }
    }
}
