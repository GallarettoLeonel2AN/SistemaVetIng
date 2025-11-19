using Microsoft.AspNetCore.Identity;
using SistemaVetIng.Data;
using SistemaVetIng.Models.Extension;  
using SistemaVetIng.Models.Indentity;
using System.Linq;                    
using System.Security.Claims;         

namespace SistemaVetIng.Models.Indentity
{
    public class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider, IConfiguration config)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Rol>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string vetAdminEmail = config["VET_ADMIN_EMAIL"];
            string vetAdminUsername = config["VET_ADMIN_USERNAME"];
            string vetAdminPassword = config["VET_ADMIN_PASSWORD"];

            string[] roles = { "Cliente", "Veterinario", "Veterinaria" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new Rol { Name = roleName });
            }

            var adminUser = await userManager.FindByEmailAsync(vetAdminEmail);

            if (adminUser == null)
            {
                adminUser = new Usuario
                {
                    UserName = vetAdminUsername,
                    Email = vetAdminEmail,
                    EmailConfirmed = true,
                    NombreUsuario = vetAdminUsername,
                };

                var result = await userManager.CreateAsync(adminUser, vetAdminPassword);

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Veterinaria");
            }

            // Crear la Veterinaria si no existe
            if (!context.Veterinarias.Any())
            {
                var nuevaVet = new Veterinaria
                {
                    UsuarioId = adminUser.Id,
                    RazonSocial = "Veterinaria Elvira",
                    Cuil = "20-43767679-9",
                    Direccion = "Av. Caferatta 1900",
                    Telefono = 1122334455
                };

                context.Veterinarias.Add(nuevaVet);
                await context.SaveChangesAsync();
            }


            #region Admin Permisos
            var adminRole = await roleManager.FindByNameAsync("Veterinaria");
            if (adminRole != null)
            {
                // Obtenemos todos los Permisos
                var allPermissions = Permission.GetAllPermissions();

                // Obtenemos los permisos que el rol ya tiene
                var currentClaims = await roleManager.GetClaimsAsync(adminRole);

                foreach (var permissionValue in allPermissions)
                {
                    
                    if (!currentClaims.Any(c => c.Type == "Permission" && c.Value == permissionValue))
                    {
                        // Si no lo tiene lo agregamos
                        var claim = new Claim("Permission", permissionValue);
                        await roleManager.AddClaimAsync(adminRole, claim);
                    }
                }
            }
            #endregion

            #region Veterinario Permisos
            var vetRole = await roleManager.FindByNameAsync("Veterinario");
            if (vetRole != null)
            {
                var defaultVeterinarioPermissions = new List<string>
            {
                Permission.Atenciones.Create,
                Permission.Atenciones.View,

                Permission.Cliente.View,
                Permission.Cliente.Create,
                Permission.Cliente.Edit,
                Permission.Cliente.Delete,

                Permission.Mascota.View,
                Permission.Mascota.Create,
                Permission.Mascota.Edit,
                Permission.Mascota.Delete,

                Permission.Turnos.View,
                Permission.Turnos.Create,
                Permission.Turnos.Cancel
            };

                var existingClaims = await roleManager.GetClaimsAsync(vetRole);

                foreach (var permission in defaultVeterinarioPermissions)
                {
                    if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                    {
                        await roleManager.AddClaimAsync(vetRole, new Claim("Permission", permission));
                    }
                }
            }

            #endregion

            #region Cliente Permisos
            var cliRole = await roleManager.FindByNameAsync("Cliente");
            if (cliRole != null)
            {
                var defaultClientePermissions = new List<string>
            {
                Permission.Atenciones.View,
               
               
                Permission.Pago.View,

                Permission.Mascota.View,
              
                Permission.Turnos.View,
                Permission.Turnos.Create,
                Permission.Turnos.Cancel
            };

                var existingClaims = await roleManager.GetClaimsAsync(cliRole);

                foreach (var permission in defaultClientePermissions)
                {
                    if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                    {
                        await roleManager.AddClaimAsync(cliRole, new Claim("Permission", permission));
                    }
                }
            }

            #endregion
        }
    }
}