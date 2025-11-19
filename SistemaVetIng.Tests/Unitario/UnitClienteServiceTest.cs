using Microsoft.AspNetCore.Identity;
using Moq;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Tests.Unitario
{
    public class UnitClienteServiceTests
    {
        private readonly Mock<IClienteRepository> _mockClienteRepo;
        private readonly Mock<UserManager<Usuario>> _mockUserManager;
        private Mock<IVeterinariaRepository> _mockVeterinariaRepo;


        private readonly ClienteService _service;

        public UnitClienteServiceTests()
        {
            // Mock del Repositorio
            _mockClienteRepo = new Mock<IClienteRepository>();
            _mockVeterinariaRepo = new Mock<IVeterinariaRepository>();

            // Mock del UserManager
            // Necesita un IUserStore falso para ser construido
            var userStoreMock = new Mock<IUserStore<Usuario>>();
            _mockUserManager = new Mock<UserManager<Usuario>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null); // Seteamos null todos los valores de userManager errors,passwordhash,etc

            // Inyectamos los Mocks en el constructor del servicio real.
            _service = new ClienteService(
                _mockClienteRepo.Object,
                _mockUserManager.Object,
                _mockVeterinariaRepo.Object
            );
        }


        [Fact]
        public async Task Registrar_DebeCrearUsuarioYCliente()
        {
            // Datos de entrada
            var viewModel = new ClienteRegistroViewModel
            {
                Email = "test@cliente.com",
                Nombre = "Test",
                Apellido = "Cliente",
                Password = "Password123!",
                Dni = 12345678,
                Telefono = 1122334455,
                Direccion = "UAI 123"
            };

            var nuevoIdUsuario = 123;

            // Mock UserManager.CreateAsync para asignar ID
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<Usuario>(), viewModel.Password))
                            .ReturnsAsync(IdentityResult.Success)
                            .Callback<Usuario, string>((usuario, password) =>
                            {
                                usuario.Id = nuevoIdUsuario;
                            });

            // Mock AddToRoleAsync
            _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<Usuario>(), "Cliente"))
                            .ReturnsAsync(IdentityResult.Success);

            // Mock Cliente Repository
            _mockClienteRepo.Setup(repo => repo.Agregar(It.IsAny<Cliente>()))
                            .Returns(Task.CompletedTask);

            _mockClienteRepo.Setup(repo => repo.Guardar())
                            .Returns(Task.CompletedTask);

            // Mock Veterinaria Repository (¡nuevo!)
            var veterinariaFake = new Veterinaria
            {
                Id = 1,
                Clientes = new List<Cliente>()
            };

            _mockVeterinariaRepo.Setup(r => r.ObtenerPrimeraAsync())
                                .ReturnsAsync(veterinariaFake);

            _mockVeterinariaRepo.Setup(r => r.Guardar())
                                .Returns(Task.CompletedTask);

            // Ejecutamos método
            var clienteResultado = await _service.Registrar(viewModel);

            // Afirmaciones del resultado
            Assert.NotNull(clienteResultado);
            Assert.Equal("Test", clienteResultado.Nombre);
            Assert.Equal(nuevoIdUsuario, clienteResultado.UsuarioId);

            // Afirmamos que se agregó correctamente al repositorio
            _mockClienteRepo.Verify(repo => repo.Agregar(It.IsAny<Cliente>()), Times.Once);
            _mockClienteRepo.Verify(repo => repo.Guardar(), Times.Once);

            // Afirmamos que la veterinaria recibió al cliente
            Assert.Single(veterinariaFake.Clientes); // ¡debe haber 1 cliente!
            Assert.Equal(clienteResultado, veterinariaFake.Clientes.First());

            // Verificamos que se guardó la veterinaria
            _mockVeterinariaRepo.Verify(v => v.Guardar(), Times.Once);
        }

        [Fact]
        public async Task Registrar_IdentityFalla_DebeLanzarExcepcionYNoCrearCliente()
        {
            var viewModel = new ClienteRegistroViewModel
            {
                Email = "fail@cliente.com",
                Password = "123"
            };

            var identityError = new IdentityError { Description = "Password demasiado corta" };

            // Mock Identity para devolver error
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Failed(identityError));

            // Ejecutar y verificar excepción
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Registrar(viewModel));
            Assert.Equal("Error al crear el usuario en Identity.", exception.Message);

            // Verificamos que NO se haya creado cliente
            _mockClienteRepo.Verify(repo => repo.Agregar(It.IsAny<Cliente>()), Times.Never());
            _mockClienteRepo.Verify(repo => repo.Guardar(), Times.Never());

            // Verificamos que NO se haya accedido a la veterinaria
            _mockVeterinariaRepo.Verify(v => v.ObtenerPrimeraAsync(), Times.Never());
        }
    }
    }
