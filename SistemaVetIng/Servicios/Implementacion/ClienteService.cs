using Microsoft.AspNetCore.Identity;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using X.PagedList;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class ClienteService : IClienteService
    {

        private readonly IClienteRepository _clienteRepository;
        private readonly IVeterinariaRepository _veterinariaRepo;
        private readonly UserManager<Usuario> _userManager;

        public ClienteService(
            IClienteRepository clienteRepository,
            UserManager<Usuario> userManager,
            IVeterinariaRepository veterinariaRepo)
        {
            _clienteRepository = clienteRepository;
            _userManager = userManager;
            _veterinariaRepo = veterinariaRepo;
        }

        #region REGISTRAR CLIENTE
        public async Task<Cliente> Registrar(ClienteRegistroViewModel viewModel)
        {
            var existingUser = await _userManager.FindByEmailAsync(viewModel.Email);
            if (existingUser != null)
                throw new Exception("El email ingresado ya está registrado.");

            if (await _clienteRepository.ExisteDniAsync(viewModel.Dni))
                throw new Exception("El DNI ingresado ya pertenece a otro cliente.");

            var usuario = new Usuario
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                NombreUsuario = $"{viewModel.Nombre} {viewModel.Apellido}",
            };

            //creaamos el usuario con la autenticación de Identity con la contraseña proporcionada en la vista.

            var result = await _userManager.CreateAsync(usuario, viewModel.Password);
            if (!result.Succeeded)
            {
                throw new Exception("Error al crear el usuario en Identity.");
            }
            await _userManager.AddToRoleAsync(usuario, "Cliente");

            // Creamos al cliente 
            var cliente = new Cliente
            {
                Nombre = viewModel.Nombre,
                Apellido = viewModel.Apellido,
                Dni = viewModel.Dni,
                Telefono = viewModel.Telefono,
                Direccion = viewModel.Direccion,
                UsuarioId = usuario.Id
            };

            // Asignamos a Veterinaria
            var veterinaria = await _veterinariaRepo.ObtenerPrimeraAsync();
            veterinaria.Clientes.Add(cliente);
            cliente.VeterinariaId = veterinaria.Id;
            await _veterinariaRepo.Guardar();

            return cliente;
        }
        #endregion

        #region MODIFICAR CLIENTE
        public async Task<Cliente> Modificar(ClienteEditarViewModel model)
        {

            var cliente = await _clienteRepository.ObtenerPorId(model.Id);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            cliente.Nombre = model.Nombre;
            cliente.Apellido = model.Apellido;
            cliente.Dni = model.Dni;
            cliente.Direccion = model.Direccion;
            cliente.Telefono = model.Telefono;

            _clienteRepository.Modificar(cliente);
            await _clienteRepository.Guardar();

            return cliente;
        }
        #endregion

        #region ELIMINAR CLIENTE
        public async Task Eliminar(int id)
        {
            var cliente = await _clienteRepository.ObtenerPorId(id);

            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            if (cliente.Usuario != null)
            {
                _clienteRepository.Eliminar(cliente);
                await _clienteRepository.Guardar();

                var result = await _userManager.DeleteAsync(cliente.Usuario);
                if (!result.Succeeded)
                {
                    throw new Exception("Error al eliminar el usuario asociado.");
                }
            }

        }
        #endregion

        #region FILTRADO CLIENTES
        public async Task<IEnumerable<Cliente>> ListarTodo()
        {
            return await _clienteRepository.ListarTodo();
        }

        public async Task<Cliente> ObtenerPorId(int id)
        {
            return await _clienteRepository.ObtenerPorId(id);
        }
        public async Task<IEnumerable<Cliente>> FiltrarPorBusqueda(string busqueda)
        {
            var clientes = await _clienteRepository.ListarTodo();

            if (!string.IsNullOrEmpty(busqueda))
            {
                return clientes.Where(c => c.Dni.ToString().Contains(busqueda));
            }

            return clientes;
        }
        public async Task<Cliente> ObtenerPorIdUsuario(int id)
        {
            return await _clienteRepository.ObtenerPorIdUsuario(id);
        }
        public async Task<Cliente> ObtenerClientePorUserNameAsync(string userName)
        {

            var usuario = await _userManager.FindByNameAsync(userName);

            if (usuario == null)
            {
                return null;
            }

            return await ObtenerPorIdUsuario(usuario.Id);
        }
        #endregion

        #region PAGINACION
        public async Task<IPagedList<Cliente>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null)
        {
            return await _clienteRepository.ListarPaginadoAsync(pageNumber, pageSize, busqueda);
        }
        #endregion


        public async Task<int> ContarTotalClientesAsync()
        {
            return await _clienteRepository.ContarTotalClientesAsync();
        }
    }
}
