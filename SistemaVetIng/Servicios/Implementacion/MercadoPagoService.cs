using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using SistemaVetIng.Servicios.Interfaces;
using MercadoPago.Client.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class MercadoPagoService : IMercadoPagoService
    {
        // Asume que la URL base de tu proyecto es localhost
        private const string BaseUrl = "https://localhost:7207"; 

        public async Task<string> CrearPreferenciaDePago(int idReferencia, decimal costoTotal, string clienteEmail, long clienteDocumento, string clienteNombre, string clienteApellido)
        {
            var item = new PreferenceItemRequest
            {
                Title = $"Costo de Atención Veterinaria #{idReferencia}",
                Quantity = 1,
                CurrencyId = "ARS", // O la moneda de tu país
                UnitPrice = costoTotal,

            };

            var backUrls = new PreferenceBackUrlsRequest
            {
                // Redirección al controlador de tu app después de pagar
                Success = $"{BaseUrl}/Pagos/ResultadoPago?estado=success&id_atencion={idReferencia}",
                Pending = $"{BaseUrl}/Pagos/ResultadoPago?estado=pending&id_atencion={idReferencia}",
                Failure = $"{BaseUrl}/Pagos/ResultadoPago?estado=failure&id_atencion={idReferencia}",
            };

            var payer = new PreferencePayerRequest
            {
                Email = clienteEmail,
                Name = clienteNombre,
                Surname = clienteApellido,
                Identification = new IdentificationRequest

                {
                    Type = "DNI", // Ajusta si usas CUIL, CUIT, etc.
                    Number = clienteDocumento.ToString(), // El número de documento del cliente
                },
            };

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest> { item },
                Payer = payer,
                BackUrls = backUrls,
                // Usamos SandBoxInitPoint en lugar de InitPoint para las pruebas
                ExternalReference = idReferencia.ToString(),
                // Para el trabajo académico, puedes dejar esta URL simulada,
                // ya que un webhook local es difícil de probar:
                NotificationUrl = $"{BaseUrl}/api/webhooks/mercadopago"
            };

            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(request);

            // Devolvemos el link de prueba (SandboxInitPoint)
            return preference.SandboxInitPoint;
        }
    }
}