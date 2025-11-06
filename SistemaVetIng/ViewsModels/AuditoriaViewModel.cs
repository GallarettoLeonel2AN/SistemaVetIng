using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVetIng.Models;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace SistemaVetIng.ViewsModels
{
    public class AuditoriaViewModel
    {
        // Filtrado
        public IPagedList<AuditoriaEvento>? EventosPaginados { get; set; }

        [Display(Name = "Usuario")]
        public string? BusquedaUsuario { get; set; }

        [Display(Name = "Tipo de Evento")]
        public string? TipoEventoSeleccionado { get; set; }

        [Display(Name = "Fecha Desde")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [Display(Name = "Fecha Hasta")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; }

        public List<SelectListItem> TiposDeEvento { get; set; }

        public AuditoriaViewModel()
        {
            EventosPaginados = new StaticPagedList<AuditoriaEvento>(new List<AuditoriaEvento>(), 1, 1, 0);

            // Precargamos la lista de filtros
            TiposDeEvento = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Todos los Tipos" },
                new SelectListItem { Value = "Login Exitoso", Text = "Login Exitoso" },
                new SelectListItem { Value = "Login Fallido", Text = "Login Fallido" },
                new SelectListItem { Value = "Logout Exitoso", Text = "Logout" },
                new SelectListItem { Value = "Crear", Text = "Creacion (ABM)" },
                new SelectListItem { Value = "Modificar", Text = "Modificacion (ABM)" },
                new SelectListItem { Value = "Eliminar", Text = "Eliminacion (ABM)" }
            };
        }
    }
}
