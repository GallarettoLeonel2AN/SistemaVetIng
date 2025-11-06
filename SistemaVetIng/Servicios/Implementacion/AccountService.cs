using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewModels;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager, 
            IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SignInResult> PasswordSignIn(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );
        }

        public async Task<string?> GetRedireccionPorRol(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return null;
            }

            if (await _userManager.IsInRoleAsync(user, "Veterinario"))
            {
                return "Veterinario";
            }
            if (await _userManager.IsInRoleAsync(user, "Cliente"))
            {
                return "Cliente";
            }
            return "Veterinaria";
        }

        public async Task<string?> GenerarPasswordResetToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Importante por seguridad: devuelve null para evitar enumeración de usuarios.
                return null;
            }
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<bool> EnviarPasswordResetEmail(string email, string callbackUrl)
        {
            try
            {
                await _emailSender.SendEmailAsync(
                    email,
                    "Restablecer Contraseña",
                    $"Hola,\n\nHemos recibido una solicitud para restablecer la contraseña de su cuenta en VetIng Software.\n\nPor favor, haga clic en el siguiente enlace para continuar con el proceso:\n\n<a href='{callbackUrl}'>Restablecer Contraseña</a>\n\nSi usted no solicitó este cambio, por favor ignore este correo. Su contraseña actual no se modificará.\n\nGracias,\nEl equipo de VetIng Software"
                );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool success, IEnumerable<IdentityError> errors)> ResetPassword(CambiarContraseñaViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return (false, new List<IdentityError>()); 
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            return (result.Succeeded, result.Errors);
        }

        public async Task<Usuario?> EncontrarUsuarioPorEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<Usuario?> EncontrarUsuarioId(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task SignOut()
        {
            await _signInManager.SignOutAsync();

            // Esto limpia cualquier cookie de "Remember Me" que haya quedado
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(
                    IdentityConstants.ExternalScheme);
            }
        }
    }
}
