using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
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
            // Crear una veterinaria para que el registro pueda asociar el cliente
            if (!await _dbContext.Veterinarias.AnyAsync())
            {
                var usuarioVet = new Usuario
                {
                    UserName = "adminVet",
                    Email = "adminVet@test.com",
                    NombreUsuario = "Adm Vet"
                };

                await _userManager.CreateAsync(usuarioVet, "Password123!");

                var veterinaria = new Veterinaria
                {
                    UsuarioId = usuarioVet.Id,
                    RazonSocial = "Vet Test",
                    Cuil = "20123456789",
                    Direccion = "Calle Falsa 123",
                    Telefono = 1144556677,
                    Clientes = new List<Cliente>(),
                    Veterinarios = new List<Veterinario>()
                };

                _dbContext.Veterinarias.Add(veterinaria);
                await _dbContext.SaveChangesAsync();
            }

            // Datos del cliente
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

            // Afirmaciones
            Assert.NotNull(clienteResultado);
            Assert.Equal("TestIntegracion", clienteResultado.Nombre);

            var clienteEnDb = await _dbContext.Clientes
                                             .FirstOrDefaultAsync(c => c.Id == clienteResultado.Id);
            Assert.NotNull(clienteEnDb);
            Assert.Equal(viewModel.Dni, clienteEnDb.Dni);

            var usuarioEnDb = await _userManager.FindByEmailAsync(viewModel.Email);
            Assert.NotNull(usuarioEnDb);
            Assert.Equal(clienteEnDb.UsuarioId, usuarioEnDb.Id);

            var roles = await _userManager.GetRolesAsync(usuarioEnDb);
            Assert.Contains("Cliente", roles);

            // Verificamos que el cliente quedó asociado a la veterinaria
            var vet = await _dbContext.Veterinarias
                                      .Include(v => v.Clientes)
                                      .FirstOrDefaultAsync();

            Assert.Contains(vet.Clientes, c => c.Id == clienteResultado.Id);
        }

    }
}
