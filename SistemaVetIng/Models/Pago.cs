using SistemaVetIng.Models;

public class Pago
{
    public int Id { get; set; }
    public int ClienteId { get; set; } 
    public Cliente Cliente { get; set; }
    public DateTime Fecha { get; set; }
    public int MetodoPagoId { get; set; } 
    public MetodoPago MetodoPago { get; set; }
    public string Estado { get; set; } 
    public decimal MontoTotal { get; set; }
    public List<AtencionVeterinaria> AtencionesCubiertas { get; set; } = new();
}