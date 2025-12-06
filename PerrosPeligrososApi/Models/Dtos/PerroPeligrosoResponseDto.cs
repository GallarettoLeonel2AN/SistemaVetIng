namespace PerrosPeligrososApi.Models.Dtos
{
    public class PerroPeligrosoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Raza { get; set; }
        public int MascotaIdOriginal { get; set; }
        public long ClienteDni { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteApellido { get; set; }
        public DateTime FechaRegistroApi { get; set; }
        public ChipResponseDto? Chip { get; set; }

        public class ChipResponseDto
        {
            public string Codigo { get; set; }
        }
    }
}
