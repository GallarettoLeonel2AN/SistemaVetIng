using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using SistemaVetIng.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SistemaVetIng;


namespace SistemaVetIng.Tests.Integracion
{
    public class WebAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Simplemente indicamos que el entorno sea Testing - Vinculacion realizada en Program
            builder.UseEnvironment("Testing");
        }
    }
}
