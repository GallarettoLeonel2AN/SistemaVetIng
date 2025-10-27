using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Threading.Tasks;

namespace SistemaVetIng.Controllers
{
    public class PagosController : Controller
    {
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly IAtencionVeterinariaService _atencionService; // Servicio para obtener datos de la atención
        private readonly IClienteService _clienteService; // Servicio para obtener datos del cliente
        private readonly IPagoService _pagoService;

        public PagosController(IMercadoPagoService mercadoPagoService, IAtencionVeterinariaService atencionService, IClienteService clienteService, IPagoService pagoService)
        {
            _mercadoPagoService = mercadoPagoService;
            _atencionService = atencionService;
            _clienteService = clienteService;
            _pagoService = pagoService;
        }

        // 1. Acción para iniciar el proceso de pago
        [HttpGet]
        // Asegúrate de inyectar tu servicio de Pagos y un DbContext (o Repositorio)
        // private readonly IPagoService _pagoService;
        // private readonly IAtencionService _atencionService; // Ya lo tenés
        // private readonly ApplicationDbContext _context; // O un repositorio

        // 1. CAMBIO: Debe ser [HttpPost] para recibir la lista de IDs en el body
        [HttpPost]
        public async Task<IActionResult> GenerarLinkDePago([FromBody] List<int> atencionesIds) // 2. CAMBIO: Recibe una LISTA de IDs
        {
            if (atencionesIds == null || !atencionesIds.Any())
            {
                return BadRequest("No se seleccionaron atenciones para pagar.");
            }

            // 3. CAMBIO: Obtenemos TODAS las atenciones de la lista (en lugar de una sola)
            // Usamos el servicio o el DbContext para traerlas.
            // Es VITAL incluir las relaciones para llegar al Cliente.
            var atencionesAPagar = await _atencionService.ObtenerVariasPorIds(atencionesIds); // (Necesitarás crear este método en tu servicio)

            // Validamos que todas existan y no estén pagadas
            if (atencionesAPagar == null || !atencionesAPagar.Any() || atencionesAPagar.Any(a => a.Abonado == true))
            {
                return NotFound("Una o más atenciones no se encontraron o ya fueron pagadas.");
            }

            // 4. CAMBIO: Nos aseguramos de que todas las atenciones sean del MISMO cliente
            var primerClienteId = atencionesAPagar.First().HistoriaClinica.Mascota.Propietario.Id;
            if (atencionesAPagar.Any(a => a.HistoriaClinica.Mascota.Propietario.Id != primerClienteId))
            {
                return BadRequest("Solo puedes pagar atenciones de un mismo cliente a la vez.");
            }

            // --- Obtenemos los datos del Cliente (igual que tu código, pero de la primera atención) ---
            var cliente = atencionesAPagar.First().HistoriaClinica.Mascota.Propietario;

            if (cliente?.Usuario == null) // 5. CAMBIO: Verificamos Dni y Usuario
            {
                return BadRequest("Falta información del cliente para procesar el pago.");
            }

            var email = cliente.Usuario.Email;
            var documento = cliente.Dni; // ¡Así se obtiene el valor de un nullable!
            var nombre = cliente.Nombre;
            var apellido = cliente.Apellido;

            // --- ¡AQUÍ EMPIEZA LA LÓGICA QUE FALTABA! ---

            // 6. Calcular el MontoTotal SUMANDO todas las atenciones
            decimal montoTotal = atencionesAPagar.Sum(a => a.CostoTotal);
            int metodoPagoAleatorioId = Random.Shared.Next(1, 4);

            // 7. Crear la entidad Pago
            var nuevoPago = new Pago
            {
                ClienteId = primerClienteId,
                Fecha = DateTime.Now,
                MontoTotal = montoTotal,
                MetodoPagoId = metodoPagoAleatorioId, //genera numero random entre 1 y 3
                Estado = "Pagado"
            };

            // Guardamos el Pago para que Entity Framework le asigne un ID
            await _pagoService.CrearPagoAsync(nuevoPago); // Necesitarás este servicio

            // 8. Actualizar TODAS las atenciones
            foreach (var atencion in atencionesAPagar)
            {
                atencion.Abonado = true; // ¡Marcada como pagada!
                atencion.PagoId = nuevoPago.Id; // ¡La vinculamos al Pago!
            }

            // Guardamos los cambios en la base de datos
            await _atencionService.ActualizarVariasAsync(atencionesAPagar); // Necesitarás este método

            // 9. Llamar al servicio de Mercado Pago (¡Ahora con el total!)
            var linkDePago = await _mercadoPagoService.CrearPreferenciaDePago(
                nuevoPago.Id, // Usamos el ID del PAGO como referencia
                montoTotal,
                email,
                documento,
                nombre,
                apellido
            );

            // 10. Redirigir al cliente (igual que antes)
            if (!string.IsNullOrEmpty(linkDePago))
            {
                return Redirect(linkDePago);
            }

            return View("Error", new { Mensaje = "No se pudo generar el link de pago." });
        }
        // 2. Acción que recibe la redirección de Mercado Pago
        [HttpGet]
        public async Task<IActionResult> ResultadoPago([FromQuery] string estado, [FromQuery] int id_atencion)
        {
            // NOTA: Esta acción es solo informativa. El estado REAL debe confirmarse con el Webhook (Punto 5).

            // Aquí podrías actualizar el estado *preliminar* en tu BD.
            // En un proyecto académico, puedes simular la actualización:
            // await _atencionService.ActualizarEstado(id_atencion, estado); 

            var viewModel = new ResultadoPagoViewModel
            {
                AtencionId = id_atencion,
                Mensaje = estado switch
                {
                    "success" => "¡Pago exitoso! La transacción está siendo procesada.",
                    "pending" => "El pago está pendiente. Recibirás una confirmación pronto.",
                    "failure" => "El pago fue rechazado. Por favor, inténtalo de nuevo.",
                    _ => "Estado de pago desconocido."
                },
                ClaseAlerta = estado switch
                {
                    "success" => "alert-success",
                    "pending" => "alert-warning",
                    "failure" => "alert-danger",
                    _ => "alert-info"
                }
            };

            // Redirige a una vista que muestre el resultado al cliente
            return View(viewModel);
        }
    }
}
