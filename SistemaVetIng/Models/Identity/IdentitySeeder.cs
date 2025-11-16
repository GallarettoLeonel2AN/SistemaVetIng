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
        }
    }
}