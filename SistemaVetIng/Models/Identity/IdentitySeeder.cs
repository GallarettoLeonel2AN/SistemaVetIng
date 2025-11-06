using Microsoft.AspNetCore.Identity;

namespace SistemaVetIng.Models.Indentity
{
    public class IdentitySeeder
    {
        // Este metodo recibe el service provider del sistema y lo usamos para acceder a los managers de usuario y roles
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // RoleManager para manejar los roles de Identity
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Rol>>();

            // UserManager para manejar usuarios de Identity
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();

            
            string[] roles = { "Cliente", "Veterinario", "Veterinaria" };

            // Recorremos cada rol y lo creamos si no existe
            foreach (var roleName in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName); 
                if (!roleExist)
                    await roleManager.CreateAsync(new Rol { Name = roleName }); 
            }

            // Creamos un usuario inicial ADMIN
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
        }
    }
}
