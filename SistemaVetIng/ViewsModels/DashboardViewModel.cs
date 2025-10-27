using SistemaVetIng.Models;

namespace SistemaVetIng.ViewsModels
{
    public class DashboardViewModel
    {
        // --- KPIs Principales  ---
        public int TotalAtencionesRealizadas { get; set; }
        public double PorcentajeAusencia { get; set; }
        public string EstadoSemaforoAusencia { get; set; } // Calculado a partir del porcentaje
        public int TotalCantidadDeTurnos { get; set; } 
        public Cliente ClienteMasFrecuente { get; set; } 
        public int TotalMascotas { get; set; }
        public int MascotasConChipCount { get; set; }
        public int PerrosPeligrosos { get; set; }
        public decimal TotalIngresosHistoricos { get; set; } 
        public decimal IngresoPromedioPorAtencion { get; set; } 
        public int TotalClientes { get; set; }

        // Conteos por Estado de Turno (para Gráfico Nivel 1) 
        public int TotalTurnosPendientes { get; set; }
        public int TotalTurnosFinalizados { get; set; }
        public int TotalTurnosCancelados { get; set; }
        public int TotalTurnosNoAsistio { get; set; }

        // --- Datos para Gráficos ---
        // Gráfico Nivel 2: Atenciones por Veterinario (reutilizamos la estructura)
        public List<AtencionesPorVeterinarioData> AtencionesPorVeterinario { get; set; } = new();
        // Gráfico Especies
        public List<EspecieCountData> DistribucionEspecies { get; set; } = new();
        // Gráfico Servicios
        public List<ServicioCountData> TopServicios { get; set; } = new();
        // Gráfico Ingresos 
        public List<IngresosAnualesData> IngresosAnuales { get; set; } = new();

        // --- Clases Internas ---
        public class AtencionesPorVeterinarioData 
        {
            public int VeterinarioId { get; set; }
            public string NombreVeterinario { get; set; }
            public int CantidadAtenciones { get; set; }
        }

        public class RazaData
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
        }

        public class EspecieCountData
        {
            public string Especie { get; set; }
            public int Cantidad { get; set; }

            public List<RazaData> Razas { get; set; } = new List<RazaData>();
        }
        public class ServicioCountData // Para Vacunas y Estudios combinados
        {
            public string NombreServicio { get; set; }
            public int CantidadSolicitudes { get; set; }
            public string Tipo { get; set; } // "Vacuna" o "Estudio"
        }
        public class IngresosAnualesData
        {
            public string Anio { get; set; }
            public decimal IngresoRealAnual { get; set; }
            public decimal MetaAnual { get; set; } 
            public string EstadoSemaforo { get; set; }
            public List<IngresosMensualesData> IngresosMensuales { get; set; } = new();
        }
        public class IngresosMensualesData
        {
            public string Mes { get; set; } // Ej: "Ene", "Feb"
            public int MesNumero { get; set; } // Ej: 1, 2 (para ordenar)
            public decimal IngresoRealMensual { get; set; }
            public decimal MetaMensual { get; set; } // Simplificado
            public string EstadoSemaforo { get; set; }
            public List<IngresosSemanalesData> IngresosSemanales { get; set; } = new();
        }
        public class IngresosSemanalesData
        {
            public string Semana { get; set; } 
            public decimal IngresoRealSemanal { get; set; }
        }

    }
}