using SistemaVetIng.Models;

public class Pago
{
    public int Id { get; set; }
    public int ClienteId { get; set; } 
    public Cliente Cliente { get; set; }
    public DateTime Fecha { get; set; }
    public int MetodoPagoId { get; set; } // 3 = Mercado Pago
    public MetodoPago MetodoPago { get; set; }
    public string Estado { get; set; } // "Pagado"

    // ¡CAMBIO IMPORTANTE!
    // Este monto será la SUMA de todas las atenciones.
    public decimal MontoTotal { get; set; }

    // ¡CAMBIO IMPORTANTE!
    // Relación "Uno a Muchos": Un Pago puede cubrir varias atenciones.
    public List<AtencionVeterinaria> AtencionesCubiertas { get; set; } = new();
}