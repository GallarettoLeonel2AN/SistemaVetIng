using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Threading.Tasks;

namespace SistemaVetIng.Controllers
{
    public class PagosController : Controller
    {
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly IAtencionVeterinariaService _atencionService; 
        private readonly IClienteService _clienteService; 
        private readonly IPagoService _pagoService;

        public PagosController(IMercadoPagoService mercadoPagoService, IAtencionVeterinariaService atencionService, IClienteService clienteService, IPagoService pagoService)
        {
            _mercadoPagoService = mercadoPagoService;
            _atencionService = atencionService;
            _clienteService = clienteService;
            _pagoService = pagoService;
        }



        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GenerarLinkDePago([FromBody] List<int> atencionesIds) 
        {
            if (atencionesIds == null || !atencionesIds.Any())
            {
                return BadRequest("No se seleccionaron atenciones para pagar.");
            }

            
           
            var atencionesAPagar = await _atencionService.ObtenerAtencionesPorIdCliente(atencionesIds); 
            var cliente = atencionesAPagar.First().HistoriaClinica.Mascota.Propietario;

            if (cliente?.Usuario == null) 
            {
                return BadRequest("Falta información del cliente para procesar el pago.");
            }

            var email = cliente.Usuario.Email;
            var documento = cliente.Dni; 
            var nombre = cliente.Nombre;
            var apellido = cliente.Apellido;

            decimal montoTotal = atencionesAPagar.Sum(a => a.CostoTotal);
            

           
            var nuevoPago = new Pago
            {
                ClienteId = cliente.Id,
                Fecha = DateTime.Now,
                MontoTotal = montoTotal,
                MetodoPagoId = 2, //id pago online
                Estado = "Pagado"
            };

           
            await _pagoService.CrearPagoAsync(nuevoPago); 

            foreach (var atencion in atencionesAPagar)
            {
                atencion.Abonado = true; 
                atencion.PagoId = nuevoPago.Id; 
            }

            
            await _atencionService.ActualizarAtencionesAsync(atencionesAPagar); 

           
            var linkDePago = await _mercadoPagoService.CrearPreferenciaDePago(
                nuevoPago.Id, // Usamos el ID del PAGO como referencia
                montoTotal,
                email,
                documento,
                nombre,
                apellido
            );

            
            if (!string.IsNullOrEmpty(linkDePago))
            {
                return Ok(new { redirectUrl = linkDePago });
            }

            return View("Error", new { Mensaje = "No se pudo generar el link de pago." });
        }
      
    }
}
