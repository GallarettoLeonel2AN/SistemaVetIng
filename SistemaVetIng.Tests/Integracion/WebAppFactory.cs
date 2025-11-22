using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using SistemaVetIng;
using SistemaVetIng.Data;
using SistemaVetIng.Models.Indentity;


namespace SistemaVetIng.Tests.Integracion
{
    public class WebAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Reemplazar DbContext real por InMemory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Crear ServiceProvider temporal para inicializar roles
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var roleManager = scopedServices.GetRequiredService<RoleManager<Rol>>();

                    db.Database.EnsureCreated();

                    string[] roles = { "Cliente", "Veterinario", "Veterinaria" };
                    foreach (var roleName in roles)
                    {
                        if (!roleManager.RoleExistsAsync(roleName).Result)
                            roleManager.CreateAsync(new Rol { Name = roleName }).Wait();
                    }
                }
            });
        }
    }
}
