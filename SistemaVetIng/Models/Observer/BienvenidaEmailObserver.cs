using Microsoft.AspNetCore.Identity.UI.Services;

namespace SistemaVetIng.Models.Observer
{
    public class BienvenidaEmailObserver : IClienteObserver
    {
        private readonly IEmailSender _emailSender;

        public BienvenidaEmailObserver(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task NotificarClienteRegistradoAsync(Cliente cliente)
        {
            string asunto = "¡Bienvenido a VetIng!";
            string mensaje = $"Hola {cliente.Nombre}, gracias por registrarte en VetIng." +
                $" Ya podes gestionar tus turnos y ver la información de tus mascotas.";

            await _emailSender.SendEmailAsync(cliente.Usuario.Email, asunto, mensaje);
        }
    }
}
