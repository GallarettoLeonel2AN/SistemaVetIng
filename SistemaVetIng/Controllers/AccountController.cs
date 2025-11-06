using MercadoPago.Resource.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewModels;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;
using System.Text.Encodings.Web; // Usado para codificar texto de forma segura en URLs.

namespace SistemaVetIng.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IAuditoriaService _auditoriaService; 
        private readonly UserManager<Usuario> _userManager;
        private readonly IToastNotification _toastNotification;

        public AccountController(IAccountService accountService,
            IAuditoriaService auditoriaService,
            UserManager<Usuario> userManager,
            IToastNotification toastNotification
            )
        {
            _accountService = accountService;
            _auditoriaService = auditoriaService;
            _userManager = userManager;
            _toastNotification = toastNotification;
        }

        [HttpGet]
        public IActionResult RecuperarContraseña() => View();

        [HttpGet]
        public IActionResult ConfirmacionEnlaceReset() => View();

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string? code = null, string? userId = null)
        {
            if (code == null || userId == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _accountService.EncontrarUsuarioId(userId);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new CambiarContraseñaViewModel { Code = code, Email = user.Email };
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(RecuperarContraseñaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var code = await _accountService.GenerarPasswordResetToken(model.Email);
            if (code == null)
            {
                
                return View("Login");
            }

            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = (await _accountService.EncontrarUsuarioPorEmail(model.Email))?.Id, code = code }, protocol: HttpContext.Request.Scheme);
            await _accountService.EnviarPasswordResetEmail(model.Email, HtmlEncoder.Default.Encode(callbackUrl));

            return View("ConfirmacionEnlaceReset");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(CambiarContraseñaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors) = await _accountService.ResetPassword(model);
            if (success)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Auditoria
           
            var user = await _userManager.FindByEmailAsync(model.UserName);

            if (user == null)
            {
                // Si el usuario no existe, registramos el intento fallido
                await _auditoriaService.RegistrarEventoAsync(
                    usuarioId: 0,
                    nombreUsuario: model.UserName,
                    tipoEvento: "Login Fallido",
                    entidad: "Sistema",
                    detalles: "Intento de login con un email que no existe."
                );
                _toastNotification.AddErrorToastMessage("Intento de inicio de sesión inválido.");
                return View(model);
            }

            model.RememberMe = false;
            var result = await _accountService.PasswordSignIn(model);

            var roles = await _userManager.GetRolesAsync(user);
            string rolUsuario = roles.FirstOrDefault() ?? "Sin Rol";

            if (result.Succeeded)
            {
                // Auditoria
                await _auditoriaService.RegistrarEventoAsync(
                    usuarioId: user.Id,
                    nombreUsuario: user.UserName,
                    tipoEvento: "Login Exitoso",
                    entidad: rolUsuario
                );

                var controllerName = await _accountService.GetRedireccionPorRol(user.UserName);

                if (controllerName != null)
                {
                    return RedirectToAction("PaginaPrincipal", controllerName);
                }

                return RedirectToAction("Index", "Home");
            }

            if (!result.Succeeded)
            {
                // Auditoria: Datos incorrectos 
                await _auditoriaService.RegistrarEventoAsync(
                    usuarioId: user.Id,
                    nombreUsuario: user.UserName,
                    tipoEvento: "Login Fallido",
                    entidad: rolUsuario,
                    detalles: "Datos incorrectos."
                );
                _toastNotification.AddErrorToastMessage("Intento de inicio de sesión inválido.");
            }
       

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? "Usuario Desconocido";
            var user = await _userManager.GetUserAsync(User);
            string rolUsuario = "Sistema";

            if (user != null) 
            {
                var roles = await _userManager.GetRolesAsync(user);
                rolUsuario = roles.FirstOrDefault() ?? "Sin Rol";
            }

            await _accountService.SignOut();

            //  Auditoria: logout
            if (int.TryParse(userIdStr, out int userId))
            {
                await _auditoriaService.RegistrarEventoAsync(
                    usuarioId: userId,
                    nombreUsuario: userName,
                    tipoEvento: "Logout Exitoso",
                    entidad: rolUsuario
                );
            }

            return RedirectToAction("Index", "Home");
        }
    }
}