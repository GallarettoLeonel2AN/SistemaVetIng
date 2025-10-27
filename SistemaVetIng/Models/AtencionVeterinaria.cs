namespace SistemaVetIng.Models
{
    public class AtencionVeterinaria
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public Tratamiento? Tratamiento { get; set; }
        public int? TratamientoId { get; set; }
        public List<Vacuna>? Vacunas { get; set; }
        public List<Estudio>? EstudiosComplementarios { get; set; }
        public string Diagnostico { get; set; }
        public decimal CostoTotal { get; set; }
        public Veterinario Veterinario { get; set; }
        public int VeterinarioId { get; set; }
        public float PesoMascota { get; set; }
        public int HistoriaClinicaId { get; set; }
        public HistoriaClinica HistoriaClinica { get; set; }
        public bool Abonado { get; set; }
        public int? PagoId { get; set; }
        public Pago Pago { get; set; }
    }
}
