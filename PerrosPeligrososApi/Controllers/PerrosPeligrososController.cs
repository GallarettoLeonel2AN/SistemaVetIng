using Microsoft.AspNetCore.Mvc;
using PerrosPeligrososApi.Models;
using PerrosPeligrososApi.Services.Interface;
using System.Net;

namespace PerrosPeligrososApi.Controllers
{
    [ApiController]
    [Route("api/perros-peligrosos")]
    public class PerrosPeligrososController : ControllerBase
    {
        private readonly ILogger<PerrosPeligrososController> _logger;
        private readonly IPerroPeligrosoService _perroPeligrosoService;

        public PerrosPeligrososController(
            ILogger<PerrosPeligrososController> logger,
            IPerroPeligrosoService perroPeligrosoService)
        {
            _logger = logger; // _logger es para registrar eventos, advertencias y errores, esencial para depurar y monitorear la API.
            _perroPeligrosoService = perroPeligrosoService;
        }


        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] PerroPeligrosoRegistroDto registroDto)
        {
            if (registroDto == null)
            {
                return BadRequest("Datos de registro no válidos.");
            }

            try
            {
                int idGenerado = await _perroPeligrosoService.ProcesarRegistro(registroDto);

                return Ok(new
                {
                    message = "Registro procesado exitosamente.",
                    apiId = idGenerado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el controlador al procesar el registro.");
                return StatusCode(500, "Error interno al procesar el registro: " + ex.Message);
            }
        }
    }
}
