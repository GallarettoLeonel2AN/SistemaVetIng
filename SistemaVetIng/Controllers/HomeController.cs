using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Models;
using SistemaVetIng.Servicios.Interfaces;
using System.Diagnostics;

namespace SistemaVetIng.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAccountService _accountService;

        public HomeController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            // Verifica si el usuario est� autenticado
            if (User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.Name;
                var controllerName = await _accountService.GetRedireccionPorRol(userName);

                if (controllerName != null)
                {
                    return RedirectToAction("PaginaPrincipal", controllerName);
                }
            }

            return View();
        }

    }
}
