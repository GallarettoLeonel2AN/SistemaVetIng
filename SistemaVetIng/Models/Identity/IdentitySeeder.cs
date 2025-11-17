using Microsoft.AspNetCore.Identity;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Models.Extension;  
using System.Security.Claims;         
using System.Linq;                    

namespace SistemaVetIng.Models.Indentity
{
    public class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Rol>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();

           
            string[] roles = { "Cliente", "Veterinario", "Veterinaria" };

            foreach (var roleName in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                    await roleManager.CreateAsync(new Rol { Name = roleName });
            }

          
            var adminEmail = "admin";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new Usuario
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    NombreUsuario = "admin",
                };
                var result = await userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Veterinaria");
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