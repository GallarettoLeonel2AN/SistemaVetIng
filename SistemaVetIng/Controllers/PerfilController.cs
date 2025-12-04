using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    public class PerfilController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ApplicationDbContext _context;

        public PerfilController(UserManager<Usuario> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(usuario);

            if (roles.Contains("Veterinario") && roles.Contains("Veterinaria"))
            {
                return RedirectToAction("VeterinarioPerfil");
            }

            if (roles.Contains("Veterinaria"))
                return RedirectToAction("VeterinariaPerfil");

            if (roles.Contains("Veterinario"))
                return RedirectToAction("VeterinarioPerfil");

            if (roles.Contains("Cliente"))
                return RedirectToAction("ClientePerfil");

            return Unauthorized();
        }

        #region VETERINARIA

        [Authorize(Roles = "Veterinaria")]
        public async Task<IActionResult> VeterinariaPerfil()
        {
            var usuario = await _userManager.GetUserAsync(User);

            var veterinaria = await _context.Veterinarias
                 .Where(v => v.UsuarioId == usuario.Id)
                 .Select(v => new VeterinariaViewModel
                 {
                   RazonSocial = v.RazonSocial ?? "",
                   Cuil = v.Cuil ?? "",
                   Direccion = v.Direccion ?? "",
                   Telefono = v.Telefono,
                   Email = usuario.Email ?? ""
                 })
                 .FirstOrDefaultAsync();

            if (veterinaria == null)
                return NotFound("No se encontró la veterinaria asociada al usuario.");

            return View(veterinaria);
        }
        #endregion

        #region VETERINARIO

        [Authorize(Roles = "Veterinario")]
        public async Task<IActionResult> VeterinarioPerfil()
        {
            var usuario = await _userManager.GetUserAsync(User);

            var veterinario = await _context.Veterinarios
                .Where(v => v.UsuarioId == usuario.Id)
                .Select(v => new VeterinarioRegistroViewModel
                {
                    Nombre = v.Nombre,
                    Apellido = v.Apellido,
                    Dni = v.Dni,
                    Direccion = v.Direccion,
                    Telefono = v.Telefono,
                    Matricula = v.Matricula,
                    Email = usuario.Email
                })
                .FirstOrDefaultAsync();

            if (veterinario == null)
                return NotFound("No se encontró el veterinario asociado a este usuario.");

            return View(veterinario);
        }
        #endregion

        #region CLIENTE

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ClientePerfil()
        {
            var usuario = await _userManager.GetUserAsync(User);

            var cliente = await _context.Clientes
                .Where(c => c.UsuarioId == usuario.Id)
                .Select(c => new ClienteRegistroViewModel
                {
                    Nombre = c.Nombre ?? "",
                    Apellido = c.Apellido ?? "",
                    Dni = c.Dni,
                    Direccion = c.Direccion ?? "",
                    Telefono = c.Telefono,
                    Email = usuario.Email ?? ""
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound("No se encontró el cliente asociado al usuario.");

            return View(cliente);
        }
        #endregion
    }
}
