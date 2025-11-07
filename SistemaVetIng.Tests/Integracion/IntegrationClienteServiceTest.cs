using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVetIng.Data;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVetIng.Tests.Integracion
{
    public class IntegrationClienteServiceTest : IClassFixture<WebAppFactory> // IClassFixture: Indica a xUnit que cree una sola instancia de WebAppFactory y la comparta entre todos los tests

    {
        private readonly IServiceScope _scope;
        private readonly IClienteService _clienteService;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<Usuario> _userManager;

        public IntegrationClienteServiceTest(WebAppFactory factory)
        {
            // Creamos un scope de servicios
            // Esto simula como ASP.NET maneja las peticiones
            _scope = factory.Services.CreateScope();

            // Obtenemos los servicios reales
            _clienteService = _scope.ServiceProvider.GetRequiredService<IClienteService>();
            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        }

        [Fact]
        public async Task Registrar_IntegrationTest_DebeCrearUsuarioYClienteEnBaseDeDatos()
        {

            var viewModel = new ClienteRegistroViewModel
            {
                Email = "test.integracion@cliente.com",
                Nombre = "TestIntegracion",
                Apellido = "Cliente",
                Password = "Password123!",
                Dni = 1235432,
                Telefono = 41343212,
                Direccion = "UAI 123"
            };

            var clienteResultado = await _clienteService.Registrar(viewModel);


            // Verificamos: metodo devolvio un cliente
            Assert.NotNull(clienteResultado);
            Assert.Equal("TestIntegracion", clienteResultado.Nombre);

            // Verificamos: El cliente existe en la base de datos en memoria
            var clienteEnDb = await _dbContext.Clientes
                                            .FirstOrDefaultAsync(c => c.Id == clienteResultado.Id);
            Assert.NotNull(clienteEnDb);
            Assert.Equal(viewModel.Dni, clienteEnDb.Dni);

            // Verificamos: El usuario de Identity existe en la base de datos
            var usuarioEnDb = await _userManager.FindByEmailAsync(viewModel.Email);
            Assert.NotNull(usuarioEnDb);
            Assert.Equal(clienteEnDb.UsuarioId, usuarioEnDb.Id);

            // Verificamos: El usuario tiene el rol cliente 
            var roles = await _userManager.GetRolesAsync(usuarioEnDb);
            Assert.Contains("Cliente", roles);
        }
    }
}
