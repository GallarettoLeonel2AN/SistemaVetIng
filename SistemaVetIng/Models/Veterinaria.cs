using SistemaVetIng.Models.Indentity;

namespace SistemaVetIng.Models
{
    public class Veterinaria
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public string RazonSocial { get; set; }
        public string Cuil { get; set; }
        public string Direccion { get; set; }
        public long Telefono { get; set; }
        public List<Veterinario> Veterinarios { get; set; }
        public List<Cliente> Clientes { get; set; }
        public ConfiguracionVeterinaria ConfiguracionVeterinaria { get; set; }
    }
}
