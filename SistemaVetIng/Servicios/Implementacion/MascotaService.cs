using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Text;
using System.Text.Json;
using X.PagedList;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class MascotaService : IMascotaService
    {
        private readonly IMascotaRepository _mascotaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IChipRepository _chipRepository;
        private readonly ApplicationDbContext _context;
        private readonly IAuditoriaService _auditoriaService;


        public MascotaService(IMascotaRepository mascotaRepository,
            IClienteRepository clienteRepository,
            IChipRepository chipRepository, 
            ApplicationDbContext context,
            IAuditoriaService auditoriaService)
        {
            _mascotaRepository = mascotaRepository;
            _clienteRepository = clienteRepository;
            _chipRepository = chipRepository;
            _context = context;
            _auditoriaService = auditoriaService;
        }

        private readonly List<string> _razasPeligrosas = new List<string>
        {
            "pitbull", "rottweiler", "dogo argentino", "fila brasileiro",
            "akita inu", "tosa inu", "doberman", "staffordshire bull terrier",
            "american staffordshire terrier", "pastor aleman"
        };
        private bool IsRazaPeligrosa(string especie, string raza)
        {
            if (string.IsNullOrEmpty(especie) || string.IsNullOrEmpty(raza))
            {
                return false;
            }

            var especieLower = especie.ToLower().Trim();
            var razaLower = raza.ToLower().Trim();

            return especieLower == "perro" && _razasPeligrosas.Contains(razaLower);
        }
        public async Task<IEnumerable<MascotaListViewModel>> ObtenerMascotasPorClienteUserNameAsync(string userName)
        {


            var mascotas = await _context.Mascotas
                .Include(m => m.Propietario)
                .ThenInclude(p => p.Usuario)
                .Where(m => m.Propietario != null && m.Propietario.Usuario.UserName == userName)
                .Select(m => new MascotaListViewModel
                {
                    Id = m.Id,
                    NombreMascota = m.Nombre,
                    Especie = m.Especie,
                    Raza = m.Raza,
                    Sexo = m.Sexo,
                    // Calcular Edad Mascota Ver Si funciona
                    EdadAnios = (DateTime.Today.Year - m.FechaNacimiento.Year),
                    RazaPeligrosa = m.RazaPeligrosa,
                    NombreDueno = m.Propietario.Nombre + " " + m.Propietario.Apellido,
                    ClienteId = m.ClienteId
                })
                .AsNoTracking()
                .ToListAsync();

            return mascotas;
        }
        public async Task<MascotaDetalleViewModel> ObtenerDetalleConHistorial(int mascotaId)
        {

            var mascotaEntity = await _context.Mascotas
                .Include(m => m.Propietario)
                .Include(m => m.Chip)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(hc => hc.Atenciones)
                        .ThenInclude(a => a.Veterinario)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(hc => hc.Atenciones)
                        .ThenInclude(a => a.Tratamiento)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(hc => hc.Atenciones)
                        .ThenInclude(a => a.Vacunas)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(hc => hc.Atenciones)
                        .ThenInclude(a => a.EstudiosComplementarios)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == mascotaId);

            if (mascotaEntity == null)
            {
                return null;
            }


            var viewModel = new MascotaDetalleViewModel
            {
                Id = mascotaEntity.Id,
                Nombre = mascotaEntity.Nombre,
                Especie = mascotaEntity.Especie,
                Raza = mascotaEntity.Raza,
                FechaNacimiento = mascotaEntity.FechaNacimiento,
                Sexo = mascotaEntity.Sexo,
                EsRazaPeligrosa = mascotaEntity.RazaPeligrosa,
                ChipCodigo = mascotaEntity.Chip?.Codigo,
                PropietarioNombreCompleto = $"{mascotaEntity.Propietario?.Nombre} {mascotaEntity.Propietario?.Apellido}",


                HistorialClinico = mascotaEntity.HistoriaClinica?.Atenciones
                    ?.OrderByDescending(a => a.Fecha)
                    .Select(a => new AtencionDetalleViewModel
                    {
                        AtencionId = a.Id,
                        Fecha = a.Fecha,
                        Diagnostico = a.Diagnostico,
                        PesoKg = a.PesoMascota,
                        VeterinarioNombreCompleto = a.Veterinario != null ? $"{a.Veterinario.Nombre} {a.Veterinario.Apellido}" : "N/A",
                        Medicamento = a.Tratamiento?.Medicamento,
                        Dosis = a.Tratamiento?.Dosis,
                        Frecuencia = a.Tratamiento?.Frecuencia,
                        DuracionDias = a.Tratamiento?.Duracion,
                        ObservacionesTratamiento = a.Tratamiento?.Observaciones,
                        NombresVacunasConLote = a.Vacunas?
                                        .Select(v => $"{v.Nombre} (Lote: {v.Lote})")
                                        .ToList() ?? new List<string>(),
                        NombresEstudios = a.EstudiosComplementarios?
                                        .Select(e => e.Nombre)
                                        .ToList() ?? new List<string>(),
                        CostoTotal = a.CostoTotal
                    }).ToList() ?? new List<AtencionDetalleViewModel>()
            };

            return viewModel;
        }
        public async Task<(Mascota? mascota, bool success, string message)> Registrar(
            MascotaRegistroViewModel model,
             int auditUserId,
            string auditUserName,
            string rolUsuario)
        {
            // Usamos una transacción para asegurar que la mascota solo se cree si la API responde correctamente.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var clienteAsociado = await _clienteRepository.ObtenerPorId(model.ClienteId);
                if (clienteAsociado == null)
                {
                    return (null, false, "El cliente asociado no es válido. Intente de nuevo.");
                }

                var mascota = new Mascota
                {
                    Nombre = model.Nombre,
                    Especie = model.Especie,
                    Raza = model.Raza,
                    FechaNacimiento = model.FechaNacimiento,
                    Sexo = model.Sexo,
                    RazaPeligrosa = IsRazaPeligrosa(model.Especie, model.Raza),
                    ClienteId = model.ClienteId,
                    HistoriaClinica = new HistoriaClinica()
                };


                await _mascotaRepository.Agregar(mascota);
                await _context.SaveChangesAsync(); 

                // Logica para el CHIP y la API de Perros Peligrosos.
                string apiMessage = string.Empty;
                if (mascota.RazaPeligrosa)
                {
                    Chip chipAsociado = null;
                    if (model.Chip)
                    {
                        chipAsociado = new Chip
                        {
                            Codigo = Guid.NewGuid().ToString("N").Substring(0, 16),
                            MascotaId = mascota.Id 
                        };

                        await _context.Chips.AddAsync(chipAsociado);
                        await _context.SaveChangesAsync();
                    }

                    bool apiCommunicationSuccess = await EnviarApiPerrosPeligrosos(
                        mascota.Id,
                        mascota.Nombre,
                        mascota.Raza,
                        mascota.RazaPeligrosa,
                        model.Chip,
                        chipAsociado?.Codigo,
                        clienteAsociado.Dni,
                        clienteAsociado.Nombre,
                        clienteAsociado.Apellido
                    );

                    if (!apiCommunicationSuccess)
                    {
                        // Si la API falla, deshacemos todo.
                        await transaction.RollbackAsync();
                        return (null, false, "Hubo un problema al comunicar con la API de perros peligrosos. La mascota no fue registrada.");
                    }

                    apiMessage = model.Chip ? $"Chip Asociado (Código: {chipAsociado?.Codigo})." : "Registrada en la API de perros peligrosos.";
                }

                // Auditoria

                await _auditoriaService.RegistrarEventoAsync(
                    usuarioId: auditUserId,
                    nombreUsuario: auditUserName,
                    tipoEvento: "Crear",
                    entidad: rolUsuario,
                    detalles: $"Se registró la mascota: {mascota.Nombre} (Raza: {mascota.Raza}) al cliente: {mascota.Propietario.Usuario.UserName}."
                );


                // Confirmamos la transacción.
                await transaction.CommitAsync();
                return (mascota, true, $"Mascota '{mascota.Nombre}' registrada correctamente. " + apiMessage);
            }
            catch (Exception ex)
            {
                // Si hay cualquier otro error, deshacemos todo.
                await transaction.RollbackAsync();
                return (null, false, $"Error al registrar la mascota: {ex.Message}");
            }
        }


        public async Task<(bool success, string message)> Modificar(MascotaEditarViewModel model)
        {

            var mascota = await _mascotaRepository.ObtenerPorId(model.Id);
            if (mascota == null)
            {
                return (false, "La mascota que intenta editar no existe.");
            }

            var chipExistente = await _chipRepository.ObtenerPorMascotaId(mascota.Id);

            if (chipExistente != null)
            {
                mascota.Chip = chipExistente;
            }

            mascota.Nombre = model.Nombre;
            mascota.Especie = model.Especie;
            mascota.Raza = model.Raza;
            mascota.FechaNacimiento = model.FechaNacimiento;
            mascota.Sexo = model.Sexo;
            mascota.RazaPeligrosa = IsRazaPeligrosa(model.Especie, model.Raza);

            try
            {

                if (mascota.RazaPeligrosa && model.Chip && mascota.Chip == null)
                {
                    // Caso 1: Se convirtió en peligrosa y se le agregó un chip.
                    var nuevoChip = new Chip
                    {
                        Codigo = Guid.NewGuid().ToString("N").Substring(0, 16),
                        MascotaId = mascota.Id
                    };

                    await _chipRepository.Agregar(nuevoChip);
                }
                else if (mascota.RazaPeligrosa && !model.Chip && mascota.Chip != null)
                {
                    // Caso 2: Era peligrosa con chip y ahora ya no tiene chip.
                    _chipRepository.Eliminar(mascota.Chip);
                    mascota.Chip = null;
                }
                else if (!mascota.RazaPeligrosa && mascota.Chip != null)
                {
                    // Si no es peligrosa, aseguramos que el chip se elimine si existía.
                    _chipRepository.Eliminar(mascota.Chip);
                    mascota.Chip = null;
                }

                // Llamar a la API si es una raza peligrosa.
                if (mascota.RazaPeligrosa)
                {
                    var clienteAsociado = await _clienteRepository.ObtenerPorId(model.ClienteId);
                    await EnviarApiPerrosPeligrosos(
                        mascota.Id,
                        mascota.Nombre,
                        mascota.Raza,
                        mascota.RazaPeligrosa,
                        model.Chip,
                        mascota.Chip?.Codigo,
                        clienteAsociado.Dni,
                        clienteAsociado.Nombre,
                        clienteAsociado.Apellido
                    );
                }

                _mascotaRepository.Modificar(mascota);
                await _mascotaRepository.Guardar();
                await _chipRepository.Guardar();

                return (true, $"Mascota '{mascota.Nombre}' actualizada correctamente.");
            }
            catch (Exception ex)
            {

                return (false, $"Error al actualizar la mascota: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> Eliminar(int id)
        {
          
            var mascota = await _context.Mascotas
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(h => h.Atenciones)
                        .ThenInclude(a => a.Tratamiento)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(h => h.Atenciones)
                        .ThenInclude(a => a.Vacunas)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(h => h.Atenciones)
                        .ThenInclude(a => a.EstudiosComplementarios)
                .Include(m => m.Chip)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mascota == null)
            {
                return (false, "La mascota que intenta eliminar no existe.");
            }

            try
            {
                // Eliminar entidades relacionadas de las atenciones.
                if (mascota.HistoriaClinica?.Atenciones != null)
                {
                    foreach (var atencion in mascota.HistoriaClinica.Atenciones.ToList())
                    {
                        if (atencion.Tratamiento != null) _context.Tratamientos.Remove(atencion.Tratamiento);
                        if (atencion.Vacunas != null) _context.Vacunas.RemoveRange(atencion.Vacunas);
                        if (atencion.EstudiosComplementarios != null) _context.Estudios.RemoveRange(atencion.EstudiosComplementarios);
                    }

                    _context.AtencionesVeterinarias.RemoveRange(mascota.HistoriaClinica.Atenciones);
                }


                if (mascota.HistoriaClinica != null)
                {
                    _context.HistoriasClinicas.Remove(mascota.HistoriaClinica);
                }


                if (mascota.Chip != null)
                {
                    _chipRepository.Eliminar(mascota.Chip);
                }

                _mascotaRepository.Eliminar(mascota);

                await _mascotaRepository.Guardar();

                return (true, "La mascota ha sido eliminada exitosamente.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error al eliminar la mascota: {ex.Message}");
                return (false, "No se pudo eliminar la mascota. Hay registros asociados o un error de base de datos.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return (false, "Ocurrió un error inesperado al eliminar la mascota.");
            }
        }

        public async Task<IEnumerable<Mascota>> ListarTodo()
        {
            return await _mascotaRepository.ListarTodo();
        }


        public async Task<Mascota> ObtenerPorId(int id)
        {
            return await _mascotaRepository.ObtenerPorId(id);
        }

        public async Task<IEnumerable<Mascota>> FiltrarPorBusqueda(string busqueda)
        {
            var mascotas = await _mascotaRepository.ListarTodo();

            if (!string.IsNullOrEmpty(busqueda))
            {
                return mascotas.Where(m => m.Propietario?.Dni.ToString().Contains(busqueda) ?? false);
            }

            return mascotas;
        }

        public async Task<IEnumerable<Mascota>> ListarMascotasPorClienteId(int clienteId)
        {
            return await _mascotaRepository.ListarMascotasPorClienteId(clienteId);
        }

        public async Task<int> ContarTotalMascotasPorClienteAsync(int idCliente)
        {
            return await _mascotaRepository.ContarTotalMascotasPorClienteAsync(idCliente);
        }

        public async Task<int> ContarPerrosChipAsync()
        {
            return await _mascotaRepository.ContarPerrosChipAsync();
        }
        public async Task<IPagedList<Mascota>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null)
        {
            return await _mascotaRepository.ListarPaginadoAsync(pageNumber, pageSize, busqueda);
        }

        public async Task<int> ContarTotalMascotasAsync()
        {
            return await _mascotaRepository.ContarTotalMascotasAsync();
        }

        public async Task<int> ContarPerrosPeligrososAsync()
        {
            return await _mascotaRepository.ContarPerrosPeligrososAsync();
        }

        public async Task<List<DashboardViewModel.EspecieCountData>> ContarMascotasPorEspecieAsync()
        {
            return await _mascotaRepository.ContarMascotasPorEspecieAsync();
        }

        public async Task<List<DashboardViewModel.RazaData>> ObtenerRazasPorEspecieAsync(string especie)
        {
            return await _mascotaRepository.ObtenerRazasPorEspecieAsync(especie);
        }

        #region API PERROSPELIGROSOS
        // Metodo para enviar datos a la API de Perros Peligrosos
        private async Task<bool> EnviarApiPerrosPeligrosos(
            int mascotaId,
            string nombreMascota,
            string razaMascota,
            bool esRazaPeligrosa,
            bool tieneChip, // Si el checkbox fue marcado
            string chipCodigo,
            long clienteDni,
            string clienteNombre,
            string clienteApellido)
        {
            // URL API 
            var apiEndpoint = "http://localhost:5075/api/perros-peligrosos/registrar";

            // Objeto de datos a enviar a API
            var dataToSend = new
            {
                MascotaId = mascotaId,
                NombreMascota = nombreMascota,
                RazaMascota = razaMascota,
                EsRazaPeligrosa = esRazaPeligrosa,
                TieneChip = tieneChip,
                ChipCodigo = chipCodigo, // Será null si no tiene chip
                ClienteDni = clienteDni,
                ClienteNombre = clienteNombre,
                ClienteApellido = clienteApellido,
                FechaRegistro = DateTime.Now
            };

            using (var client = new HttpClient())
            {

                // Clave para el Authorize
                client.DefaultRequestHeaders.Add("PERROPELIGROSO-API-KEY", "AccesoVetIng");

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(dataToSend),
                    Encoding.UTF8,
                    "application/json"
                );

                try
                {
                    Console.WriteLine($"Enviando a API de Perros Peligrosos: {jsonContent.ReadAsStringAsync().Result}");
                    var response = await client.PostAsync(apiEndpoint, jsonContent);

                    // Si la API retorna un código de estado de exito
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Respuesta exitosa de la API: {await response.Content.ReadAsStringAsync()}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Error de API ({response.StatusCode}): {await response.Content.ReadAsStringAsync()}");
                        return false;
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    // Errores de red,DNS,conexión rechazada,etc
                    Console.WriteLine($"Error de conexión HTTP con la API: {httpEx.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    // Otros errores (serialización,etc)
                    Console.WriteLine($"Error general al enviar datos a la API: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

    }
}
