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

        private readonly ClienteService _service;

        public UnitClienteServiceTests()
        {
            // Mock del Repositorio
            _mockClienteRepo = new Mock<IClienteRepository>();

            // Mock del UserManager
            // Necesita un IUserStore falso para ser construido
            var userStoreMock = new Mock<IUserStore<Usuario>>();
            _mockUserManager = new Mock<UserManager<Usuario>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null); // Seteamos null todos los valores de userManager errors,passwordhash,etc

            // Inyectamos los Mocks en el constructor del servicio real.
            _service = new ClienteService(
                _mockClienteRepo.Object,
                _mockUserManager.Object
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

            var nuevoIdUsuario = 123; // ID simulado que asignara a Identity

            // Configurar el Mock de UserManager para que devuelva exito
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<Usuario>(), viewModel.Password))
                            .ReturnsAsync(IdentityResult.Success)
                            // Callback simula que Identity asigna un ID al usuario creado
                            .Callback<Usuario, string>((usuario, password) =>
                            {
                                usuario.Id = nuevoIdUsuario;
                            });

            // Configurar el Mock de UserManager para que AddRole también funcione
            _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<Usuario>(), "Cliente"))
                            .ReturnsAsync(IdentityResult.Success);

            // Configurar el Mock del Repositorio para que no haga nada ya que son void/Task
            _mockClienteRepo.Setup(repo => repo.Agregar(It.IsAny<Cliente>()))
                            .Returns(Task.CompletedTask); // Para metodos async Task sin return
            _mockClienteRepo.Setup(repo => repo.Guardar())
                            .Returns(Task.CompletedTask);



            // Ejecutamos metodo
            var clienteResultado = await _service.Registrar(viewModel);

            // Afirmaciones

            Assert.NotNull(clienteResultado);
            Assert.Equal("Test", clienteResultado.Nombre);
            Assert.Equal(nuevoIdUsuario, clienteResultado.UsuarioId);

            // Nos aseguramos de que el repositorio si intento agregar un cliente y guardar cambios
            _mockClienteRepo.Verify(repo => repo.Agregar(It.IsAny<Cliente>()), Times.Once);
            _mockClienteRepo.Verify(repo => repo.Guardar(), Times.Once);
        }


        [Fact]
        public async Task Registrar_IdentityFalla_DebeLanzarExcepcionYNoCrearCliente()
        {

            var viewModel = new ClienteRegistroViewModel { Email = "fail@cliente.com", Password = "p" };
            var identityError = new IdentityError { Description = "Password demasiado corta" };

            // Configuramos el Mock de UserManager para que devuelva Error
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Failed(identityError));


            // Verificamos que el metodo lance la excepcion
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.Registrar(viewModel)
            );

            // Verificamos que el mensaje de la excepcion sea el correcto
            Assert.Equal("Error al crear el usuario en Identity.", exception.Message);

            // VERIFICAMOS QUE NUNCA SE HAYA INTENTADO GUARDAR EL CLIENTE
            _mockClienteRepo.Verify(repo => repo.Agregar(It.IsAny<Cliente>()), Times.Never());
            _mockClienteRepo.Verify(repo => repo.Guardar(), Times.Never());
        }
    }
}
