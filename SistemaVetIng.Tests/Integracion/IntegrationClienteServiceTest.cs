using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
            _scope = factory.Services.CreateScope();
            _clienteService = _scope.ServiceProvider.GetRequiredService<IClienteService>();
            _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        }

        [Fact]
        public async Task Registrar_IntegrationTest_DebeCrearUsuarioYClienteEnBaseDeDatos()
        {
            // Crear veterinaria si no existe
            await EnsureVeterinariaExists();

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

            Assert.NotNull(clienteResultado);
            Assert.Equal("TestIntegracion", clienteResultado.Nombre);

            var clienteEnDb = await _dbContext.Clientes
                                             .FirstOrDefaultAsync(c => c.Id == clienteResultado.Id);
            Assert.NotNull(clienteEnDb);
            Assert.Equal(viewModel.Dni, clienteEnDb.Dni);

            var usuarioEnDb = await _userManager.FindByEmailAsync(viewModel.Email);
            Assert.NotNull(usuarioEnDb);

            var roles = await _userManager.GetRolesAsync(usuarioEnDb);
            Assert.Contains("Cliente", roles);

            var vet = await _dbContext.Veterinarias
                                      .Include(v => v.Clientes)
                                      .FirstOrDefaultAsync();
            Assert.Contains(vet.Clientes, c => c.Id == clienteResultado.Id);
        }



        // Metodo para crear Veterinaria
        private async Task EnsureVeterinariaExists()
        {
            // Verifica si la veterinaria ya existe en la BD In-Memory
            if (await _dbContext.Veterinarias.AnyAsync())
            {
                return; 
            }

            // Si no existe, la creamos
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


        [Fact]
        public async Task Registrar_IntegrationTest_DniDuplicado_DebeLanzarExcepcion()
        {
            await EnsureVeterinariaExists();

            var primerCliente = new ClienteRegistroViewModel
            {
                Email = "dni.original@test.com",
                Nombre = "Original",
                Apellido = "Original",
                Password = "Password123!",
                Dni = 99887766, // DNI que se duplicará
                Telefono = 11111111,
                Direccion = "Dir 1"
            };

            await _clienteService.Registrar(primerCliente);

            var clienteDuplicado = new ClienteRegistroViewModel
            {
                Email = "nuevo.email.unico@test.com", // Email diferente (para pasar la primera validación)
                Nombre = "Duplicado",
                Apellido = "Duplicado",
                Password = "Password123!",
                Dni = 99887766, // Ya existe
                Telefono = 22222222,
                Direccion = "Dir 2"
            };

            //  Intentar registrar el duplicado y esperar la excepción
            var ex = await Assert.ThrowsAsync<Exception>(() => _clienteService.Registrar(clienteDuplicado));

            Assert.Equal("El DNI ingresado ya pertenece a otro cliente.", ex.Message);

            // El usuario duplicado NO debe existir en la BD
            var usuarioDuplicado = await _userManager.FindByEmailAsync(clienteDuplicado.Email);
            Assert.Null(usuarioDuplicado);
        }
    }
    }
