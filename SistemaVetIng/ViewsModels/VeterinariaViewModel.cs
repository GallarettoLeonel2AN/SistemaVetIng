using SistemaVetIng.Models.Indentity;

namespace SistemaVetIng.ViewsModels
{
    public class VeterinariaViewModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public string RazonSocial { get; set; }
        public string Cuil { get; set; }
        public string Direccion { get; set; }
        public long Telefono { get; set; }
        public string Email { get; set; }
    }
}
