using Microsoft.AspNetCore.Identity;
using Moq;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Models.Observer;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Tests.Unitario
{
    public class UnitClienteServiceTests
    {
        private readonly Mock<IClienteRepository> _mockClienteRepo;
        private readonly Mock<UserManager<Usuario>> _mockUserManager;
        private readonly Mock<IVeterinariaRepository> _mockVeterinariaRepo;
        private readonly Mock<IClienteSubject> _mockClienteSubject;

        private readonly ClienteService _service;

        public UnitClienteServiceTests()
        {
            // Mock repositorios
            _mockClienteRepo = new Mock<IClienteRepository>();
            _mockVeterinariaRepo = new Mock<IVeterinariaRepository>();
            _mockClienteSubject = new Mock<IClienteSubject>();

            // Mock UserManager 
            var store = new Mock<IUserStore<Usuario>>();
            _mockUserManager = new Mock<UserManager<Usuario>>(
                store.Object, null, null, null, null, null, null, null, null);

            // Servicio 
            _service = new ClienteService(
                _mockClienteRepo.Object,
                _mockUserManager.Object,
                _mockVeterinariaRepo.Object,
                _mockClienteSubject.Object
            );
        }

        [Fact]
        public async Task Registrar_DebeCrearUsuarioYCliente()
        {
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

            // Mock creacion Identity
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<Usuario>(), viewModel.Password))
                            .ReturnsAsync(IdentityResult.Success)
                            .Callback<Usuario, string>((usuario, pass) => usuario.Id = nuevoIdUsuario);

            // Mock rol
            _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<Usuario>(), "Cliente"))
                            .ReturnsAsync(IdentityResult.Success);

            // Mock repo cliente
            _mockClienteRepo.Setup(r => r.Agregar(It.IsAny<Cliente>()))
                            .Returns(Task.CompletedTask);

            _mockClienteRepo.Setup(r => r.Guardar())
                            .Returns(Task.CompletedTask);

            // Mock veterinaria
            var vetFake = new Veterinaria
            {
                Id = 1,
                Clientes = new List<Cliente>()
            };

            _mockVeterinariaRepo.Setup(r => r.ObtenerPrimeraAsync())
                                .ReturnsAsync(vetFake);

            _mockVeterinariaRepo.Setup(r => r.Guardar())
                                .Returns(Task.CompletedTask);

            // Mock subject 
            _mockClienteSubject.Setup(s => s.NotificarAsync(It.IsAny<Cliente>()))
                               .Returns(Task.CompletedTask);

            // Ejecutar
            var clienteResultado = await _service.Registrar(viewModel);

            // Asserts
            Assert.NotNull(clienteResultado);
            Assert.Equal("Test", clienteResultado.Nombre);
            Assert.Equal(nuevoIdUsuario, clienteResultado.UsuarioId);

            Assert.Single(vetFake.Clientes);
            Assert.Equal(clienteResultado, vetFake.Clientes.First());

            _mockVeterinariaRepo.Verify(v => v.Guardar(), Times.Once);

            // Assert Observer
            _mockClienteSubject.Verify(s => s.NotificarAsync(It.IsAny<Cliente>()), Times.Once);
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

            // Mock error Identity
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Failed(identityError));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Registrar(viewModel));

            Assert.Equal("Error al crear el usuario en Identity.", ex.Message);

            _mockClienteRepo.Verify(repo => repo.Agregar(It.IsAny<Cliente>()), Times.Never());
            _mockVeterinariaRepo.Verify(v => v.ObtenerPrimeraAsync(), Times.Never());

            // Observer NO debe dispararse
            _mockClienteSubject.Verify(s => s.NotificarAsync(It.IsAny<Cliente>()), Times.Never());
        }
    }

}
