﻿namespace SistemaVetIng.Models
{
    public class Vacuna
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
      
        public string Lote { get; set; }

        public DateTime FechaAplicacion { get; set; }

        
        public decimal Precio { get; set; }

        public int? AtencionVeterinariaId { get; set; } 
        public AtencionVeterinaria? AtencionVeterinaria { get; set; } 

    }
}
