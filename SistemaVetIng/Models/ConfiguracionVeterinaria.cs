namespace SistemaVetIng.Models
{
    public class ConfiguracionVeterinaria
    {
        public int Id { get; set; }
        public int DuracionMinutosPorConsulta { get; set; }
        public int VeterinariaId { get; set; }
        public Veterinaria Veterinaria { get; set; }
        public virtual ICollection<HorarioDia> HorariosPorDia { get; set; } = new List<HorarioDia>();
    }
}
