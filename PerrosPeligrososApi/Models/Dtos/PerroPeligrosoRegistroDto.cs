namespace PerrosPeligrososApi.Models.Dtos
{
    public class PerroPeligrosoRegistroDto
    {
        public int MascotaId { get; set; }
        public string NombreMascota { get; set; }
        public string RazaMascota { get; set; }
        public bool EsRazaPeligrosa { get; set; } 
        public bool TieneChip { get; set; }
        public string? ChipCodigo { get; set; } // Puede ser nulo si no tiene chip
        public long ClienteDni { get; set; }
        public string? ClienteNombre { get; set; }
        public string? ClienteApellido { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
