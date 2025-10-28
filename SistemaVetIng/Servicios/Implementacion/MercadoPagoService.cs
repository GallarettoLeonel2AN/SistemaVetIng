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
        
        private const string BaseUrl = "https://localhost:7207"; 

        public async Task<string> CrearPreferenciaDePago(int idReferencia, decimal costoTotal, string clienteEmail, long clienteDocumento, string clienteNombre, string clienteApellido)
        {
            var item = new PreferenceItemRequest
            {
                Title = $"Costo de Atención Veterinaria #{idReferencia}",
                Quantity = 1,
                CurrencyId = "ARS", 
                UnitPrice = costoTotal,

            };

            var backUrls = new PreferenceBackUrlsRequest
            {
               
                Success = $"{BaseUrl}/Cliente/PaginaPrincipal",
                Pending = $"{BaseUrl}/Cliente/PaginaPrincipal",
                Failure = $"{BaseUrl}/Cliente/PaginaPrincipal",
            };

            var payer = new PreferencePayerRequest
            {
                Email = clienteEmail,
                Name = clienteNombre,
                Surname = clienteApellido,
                Identification = new IdentificationRequest

                {
                    Type = "DNI", 
                    Number = clienteDocumento.ToString(), 
                },
            };

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest> { item },
                Payer = payer,
                BackUrls = backUrls,
            
                ExternalReference = idReferencia.ToString(),
                NotificationUrl = $"{BaseUrl}/api/webhooks/mercadopago"
            };

            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(request);

           
            return preference.SandboxInitPoint;
        }
    }
}