using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SistemaVetIng.Controllers
{

    [Route("api/webhooks/mercadopago")]
    [ApiController]
    public class MercadoPagoWebhookController : ControllerBase
    {
       

        [HttpPost]
        public IActionResult ReceiveNotification([FromQuery] string id, [FromQuery] string topic)
        {
        
            if (topic == "payment" && !string.IsNullOrEmpty(id))
            {
                
                return Ok();
            }

            return BadRequest();
        }
    }
}
