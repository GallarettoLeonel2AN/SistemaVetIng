﻿using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc; 
using SistemaVetIng.Data; 
using SistemaVetIng.Models.Indentity; 
using SistemaVetIng.Models; 
using SistemaVetIng.ViewsModels; 

namespace SistemaVetIng.Controllers
{

    public class ClienteController : Controller
    {
        private readonly UserManager<Usuario> _userManager; 
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ApplicationDbContext _context;

        public ClienteController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // Mostrar el formulario
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

       // Procesar el registro
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevenir ataques de falsificación de solicitudes
        public async Task<IActionResult> Registro(ClienteRegistroViewModel model)
        {
            // Validamos reglas del viewmodel
            if (!ModelState.IsValid)
                return View(model); 

     
            var usuario = new Usuario
            {
                UserName = model.Email, // Usamos el email como nombre de usuario.
                Email = model.Email,
                NombreUsuario = model.Nombre + "" + model.Apellido, // Combinamos nombre y apellido para el nombre de usuario.
            };

            // Intentamos crear el usuario en el sistema de autenticación de Identity con la contraseña proporcionada.
            var result = await _userManager.CreateAsync(usuario, model.Password);

            // Si la creación del usuario falla
            if (!result.Succeeded)
            {
                // Agregamos los errores de Identity al ModelState para que se muestren en la vista.
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                TempData["Error"] = "Hubo errores al crear el usuario."; 
                return View(model); 
            }

            // Si el usuario se creó con exito en Identity, le asignamos el rol "Cliente".
            await _userManager.AddToRoleAsync(usuario, "Cliente");

            // Creamos al cliente 
            var cliente = new Cliente
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Dni = model.Dni,
                Telefono = model.Telefono,
                Direccion = model.Direccion,
                UsuarioId = usuario.Id, // Vinculación clave con la cuenta de Identity.
            };

            try
            {
                _context.Clientes.Add(cliente); // Agregamos el nuevo cliente al contexto.
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "¡Cliente registrado con éxito!";

                // Redirigimos al usuario al inicio
                return RedirectToAction("Index", "Home");
            }
            // Si hay Error
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Registramos el error completo en la consola
                Console.WriteLine("--- ERROR AL GUARDAR EN LA BASE DE DATOS ---");
                Console.WriteLine($"Error principal: {ex.Message}");
                Console.WriteLine($"Error interno: {ex.InnerException?.Message}"); 
                Console.WriteLine("------------------------------------------");

                // Mostramos un mensaje de error
                ModelState.AddModelError(string.Empty, "Hubo un error al guardar el cliente. Es posible que el DNI ya esté registrado.");

                return View(model);
            }
        }
    }
}