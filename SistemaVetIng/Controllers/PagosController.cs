using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Threading.Tasks;

namespace SistemaVetIng.Controllers
{
    public class PagosController : Controller
    {
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly IAtencionVeterinariaService _atencionService; 
        private readonly IPagoService _pagoService;
        private readonly IToastNotification _toastNotification;

        public PagosController(IMercadoPagoService mercadoPagoService,
            IAtencionVeterinariaService atencionService, 
            IPagoService pagoService,
            IToastNotification toastNotification)
        {
            _mercadoPagoService = mercadoPagoService;
            _atencionService = atencionService;
            _pagoService = pagoService;
            _toastNotification = toastNotification;
        }


        #region GENERAR LINK PAGO
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GenerarLinkDePago([FromBody] List<int> atencionesIds) 
        {
            if (atencionesIds == null || !atencionesIds.Any())
            {
                _toastNotification.AddErrorToastMessage("No se seleccionaron atenciones para pagar.");
            }

            
           
            var atencionesAPagar = await _atencionService.ObtenerAtencionesPorIdCliente(atencionesIds); 
            var cliente = atencionesAPagar.First().HistoriaClinica.Mascota.Propietario;

            if (cliente?.Usuario == null) 
            {
                _toastNotification.AddErrorToastMessage("Falta información del cliente para procesar el pago.");
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
                _toastNotification.AddSuccessToastMessage("Pago realizado correctamente");
                return Ok(new { redirectUrl = linkDePago });
            }

            return View("Error", new { Mensaje = "No se pudo generar el link de pago." });
        }

        #endregion

        #region METODO DE PAGOS

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarEfectivo(int atencionId, decimal monto, int clienteId, int mascotaId)
        {
            var success = await _pagoService.CrearPagoPresencialAsync(atencionId, clienteId, monto, 1); // 1 = Efectivo

            if (success)
            {
                _toastNotification.AddSuccessToastMessage("Pago en Efectivo registrado con éxito.");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Error al registrar el pago.");
            }

            return RedirectToAction("DetalleHistoriaClinica", "HistoriaClinica", new { mascotaId = mascotaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarTarjeta(int atencionId, decimal monto, int clienteId, int mascotaId)
        {
            var success = await _pagoService.CrearPagoPresencialAsync(atencionId, clienteId, monto, 3); // 3 = Tarjeta

            if (success)
            {
                _toastNotification.AddSuccessToastMessage("Pago con Tarjeta registrado con éxito.");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Error al registrar el pago.");
            }

            return RedirectToAction("DetalleHistoriaClinica", "HistoriaClinica", new { mascotaId = mascotaId });
        }

        #endregion

    }
}
