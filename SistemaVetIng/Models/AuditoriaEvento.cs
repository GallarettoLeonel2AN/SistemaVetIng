using SistemaVetIng.Models.Indentity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVetIng.Models
{
    // Representamos un registro de auditoria generico para cualquier evento
    // (Login, Logout, Crear, Modificar, Eliminar)
    public class AuditoriaEvento
    {
        [Key]
        public long Id { get; set; } 
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(256)]
        public string NombreUsuario { get; set; }

        // El tipo de evento (ej: "Login Exitoso", "Logout", "Crear", "Modificar", "Eliminar")
        [Required]
        [StringLength(100)]
        public string TipoEvento { get; set; }

        // El rol que lo realizo (Cliente, Veterinario, Veterinaria)
        [Required]
        [StringLength(100)]
        public string Entidad { get; set; }

        public DateTime FechaHora { get; set; }

        public string? Detalles { get; set; }

        // Propiedad de navegacion 

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }
    }
}