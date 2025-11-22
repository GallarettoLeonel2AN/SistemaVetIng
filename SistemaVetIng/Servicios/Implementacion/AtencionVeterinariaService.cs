using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Memento;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class AtencionVeterinariaService : IAtencionVeterinariaService
    {
        private readonly IAtencionVeterinariaRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly IVeterinarioService _veterinarioService;
        private readonly IVacunaService _vacunaService;
        private readonly IEstudioService _estudioService;
        private readonly ITurnoService _turnoService;
        private readonly IHistoriaClinicaService _historiaClinicaService;

        public AtencionVeterinariaService(
            IAtencionVeterinariaRepository repository,
            ApplicationDbContext context,
            IVeterinarioService veterinarioService,
            IVacunaService vacunaService,
            IEstudioService estudioService,
            ITurnoService turnoService,
            IHistoriaClinicaService historiaClinicaService)
        {
            _repository = repository;
            _context = context;
            _veterinarioService = veterinarioService;
            _vacunaService = vacunaService;
            _estudioService = estudioService;
            _turnoService = turnoService;
            _historiaClinicaService = historiaClinicaService;
        }

        public async Task<AtencionVeterinariaViewModel> GetAtencionVeterinariaViewModel(int historiaClinicaId)
        {
            var historiaClinica = await _historiaClinicaService.GetHistoriaClinicaConMascotayPropietario(historiaClinicaId);

            if (historiaClinica == null)
            {
                return null;
            }

            var viewModel = new AtencionVeterinariaViewModel
            {
                HistoriaClinicaId = historiaClinicaId
            };

            // Pasar datos para la vista 
            viewModel.MascotaNombre = historiaClinica.Mascota.Nombre;
            viewModel.PropietarioNombre = $"{historiaClinica.Mascota.Propietario?.Nombre} {historiaClinica.Mascota.Propietario?.Apellido}";
            viewModel.MascotaId = historiaClinica.Mascota.Id;

            // Obtener datos para SelectList
            var vacunas = await _vacunaService.ListarTodo();
            var estudios = await _estudioService.ListarTodo();

            viewModel.VacunasDisponibles = new SelectList(vacunas, "Id", "Nombre");
            viewModel.EstudiosDisponibles = new SelectList(estudios, "Id", "Nombre");
            viewModel.VacunasConPrecio = vacunas.Select(v => new { v.Id, v.Nombre, v.Precio }).ToList();
            viewModel.EstudiosConPrecio = estudios.Select(e => new { e.Id, e.Nombre, e.Precio }).ToList();

            return viewModel;
        }

        public async Task<string> CreateAtencionVeterinaria(AtencionVeterinariaViewModel model, ClaimsPrincipal user)
        {

            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userIdInt))
            {
                return "Error al obtener el ID del usuario.";
            }

            var veterinario = await _veterinarioService.ObtenerPorIdUsuario(userIdInt);
            if (veterinario == null)
            {
                return "El usuario logueado no está asociado a un perfil de veterinario.";
            }

            model.VeterinarioId = veterinario.Id;

            // Obtener vacunas y estudios y calcular costos
            var vacunasSeleccionadas = await _vacunaService.GetVacunaSeleccionada(model.VacunasSeleccionadasIds);
            var estudiosSeleccionados = await _estudioService.GetEstudioSeleccionado(model.EstudiosSeleccionadosIds);

            decimal costoVacunas = vacunasSeleccionadas.Sum(v => v.Precio);
            decimal costoEstudios = estudiosSeleccionados.Sum(e => e.Precio);
            decimal costoConsultaBase = 5000;
            decimal costoTotal = costoVacunas + costoConsultaBase + costoEstudios;

            // Crear tratamiento
            Tratamiento? tratamiento = null;
            if (!string.IsNullOrEmpty(model.Medicamento) || !string.IsNullOrEmpty(model.Dosis))
            {
                tratamiento = new Tratamiento
                {
                    Medicamento = model.Medicamento,
                    Dosis = model.Dosis,
                    Frecuencia = model.Frecuencia,
                    Duracion = model.DuracionDias,
                    Observaciones = model.ObservacionesTratamiento
                };
                await _repository.AgregarTratamiento(tratamiento);
            }

            // Crear la atención
            var atencion = new AtencionVeterinaria
            {
                Fecha = DateTime.Now,
                Diagnostico = model.Diagnostico,
                PesoMascota = model.PesoKg,
                HistoriaClinicaId = model.HistoriaClinicaId,
                VeterinarioId = model.VeterinarioId,
                Tratamiento = tratamiento,
                CostoTotal = costoTotal,
                Vacunas = vacunasSeleccionadas,
                EstudiosComplementarios = estudiosSeleccionados
            };

            await _repository.AgregarAtencionVeterinaria(atencion);
            await _repository.SaveChangesAsync();

            return null;
        }
        public async Task<AtencionVeterinaria> ObtenerPorId(int id)
        {
            return await _repository.ObtenerAtencionConCliente(id);
        }
        public async Task<List<AtencionDetalleViewModel>> ObtenerPagosPendientesPorClienteId(int clienteId)
        {
            var atencionesDB = await _repository.ObtenerAtencionesPendientesPorCliente(clienteId);


            var viewModelList = atencionesDB
                .Select(a => new AtencionDetalleViewModel
                {
                    AtencionId = a.Id,
                    CostoTotal = a.CostoTotal,
                    EstadoDePago = "Pendiente",
                    Fecha = a.Fecha,
                    MascotaNombre = a.HistoriaClinica.Mascota.Nombre
                })
                .ToList();

            return viewModelList;
        }
        public async Task RegistrarAtencionDesdeTurnoAsync(AtencionPorTurnoViewModel model, ClaimsPrincipal user)
        {
            // Usamos una transacción para garantizar la integridad de los datos.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Obtener veterinario
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userIdInt))
                {
                    throw new Exception("Error al obtener el ID del usuario.");
                }

                var veterinario = await _veterinarioService.ObtenerPorIdUsuario(userIdInt);
                if (veterinario == null)
                {
                    throw new Exception("El usuario logueado no está asociado a un perfil de veterinario.");
                }

                model.VeterinarioId = veterinario.Id;

                // Costos
                var vacunasSeleccionadas = await _vacunaService.ObtenerPorIdsAsync(model.VacunasSeleccionadasIds);
                var estudiosSeleccionados = await _estudioService.ObtenerPorIdsAsync(model.EstudiosSeleccionadosIds);

                decimal costoVacunas = vacunasSeleccionadas.Sum(v => v.Precio);
                decimal costoEstudios = estudiosSeleccionados.Sum(e => e.Precio);
                decimal costoConsultaBase = 1000;
                decimal costoTotal = costoVacunas + costoConsultaBase + costoEstudios;

                // Tratamiento
                Tratamiento tratamiento = null;
                if (!string.IsNullOrWhiteSpace(model.Medicamento))
                {
                    tratamiento = new Tratamiento
                    {
                        Medicamento = model.Medicamento,
                        Dosis = model.Dosis,
                        Frecuencia = model.Frecuencia,
                        Duracion = model.DuracionDias.ToString(),
                        Observaciones = model.ObservacionesTratamiento
                    };
                    await _repository.AgregarTratamiento(tratamiento);
                }

                // Atencion
                var atencion = new AtencionVeterinaria
                {
                    Fecha = DateTime.Now,
                    Diagnostico = model.Diagnostico,
                    PesoMascota = (float)model.PesoKg,
                    HistoriaClinicaId = model.HistoriaClinicaId,
                    VeterinarioId = veterinario.Id,
                    Tratamiento = tratamiento,
                    CostoTotal = costoTotal,
                    Vacunas = vacunasSeleccionadas.ToList(),
                    EstudiosComplementarios = estudiosSeleccionados.ToList()
                };

                await _repository.AgregarAtencionVeterinaria(atencion);
                await _context.SaveChangesAsync();

                // Actualizar Turno
                var turno = await _turnoService.ObtenerPorIdConDatosAsync(model.TurnoId);
                if (turno == null)
                {
                    throw new Exception("El turno asociado no fue encontrado.");
                }

                turno.Estado = "Finalizado";

                _turnoService.Actualizar(turno);
                await _context.SaveChangesAsync();

                // Transaccion
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #region Memento
        // Metodo para editar (Guardando el Memento antes)
        public async Task EditarAtencionConRespaldoAsync(AtencionVeterinariaViewModel model, ClaimsPrincipal user, string motivo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Buscar por ID
                var atencionActual = await _context.AtencionesVeterinarias
                    .Include(a => a.Tratamiento)
                    .FirstOrDefaultAsync(a => a.Id == model.Id);

                if (atencionActual == null) throw new Exception("Atención no encontrada");

                // Crear memento (Guardar estado anterior)
                var memento = new AtencionVeterinariaMemento
                {
                    AtencionVeterinariaId = atencionActual.Id,
                    FechaVersion = DateTime.Now,
                    UsuarioEditor = user.Identity?.Name ?? "Desconocido",
                    MotivoCambio = motivo,

                    // Copiamos datos viejos
                    Diagnostico = atencionActual.Diagnostico,
                    PesoMascota = atencionActual.PesoMascota,

                    // Datos del tratamiento viejo
                    TratamientoMedicamento = atencionActual.Tratamiento?.Medicamento,
                    TratamientoDosis = atencionActual.Tratamiento?.Dosis,
                    TratamientoFrecuencia = atencionActual.Tratamiento?.Frecuencia,
                    TratamientoDuracion = atencionActual.Tratamiento?.Duracion,
                    TratamientoObservaciones = atencionActual.Tratamiento?.Observaciones
                };

                _context.AtencionMementos.Add(memento);
                await _context.SaveChangesAsync();

                // Actualizar con los datos nuevos
                atencionActual.Diagnostico = model.Diagnostico;
                atencionActual.PesoMascota = model.PesoKg;

                // Logica de Tratamiento
                if (atencionActual.Tratamiento != null)
                {
                    atencionActual.Tratamiento.Medicamento = model.Medicamento;
                    atencionActual.Tratamiento.Dosis = model.Dosis;
                    atencionActual.Tratamiento.Frecuencia = model.Frecuencia;
                    atencionActual.Tratamiento.Duracion = model.DuracionDias;
                    atencionActual.Tratamiento.Observaciones = model.ObservacionesTratamiento;
                }
                else if (!string.IsNullOrEmpty(model.Medicamento))
                {
                    var nuevoTratamiento = new Tratamiento
                    {
                        Medicamento = model.Medicamento,
                        Dosis = model.Dosis,
                        Frecuencia = model.Frecuencia,
                        Duracion = model.DuracionDias,
                        Observaciones = model.ObservacionesTratamiento
                    };
                    _context.Tratamientos.Add(nuevoTratamiento);
                    atencionActual.Tratamiento = nuevoTratamiento;
                }

                _context.AtencionesVeterinarias.Update(atencionActual);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<AtencionVeterinariaViewModel> ObtenerAtencionParaEditarAsync(int id)
        {
            var atencion = await _context.AtencionesVeterinarias
                .Include(a => a.Tratamiento)
                .Include(a => a.Vacunas) 
                .Include(a => a.EstudiosComplementarios)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (atencion == null) return null;

           
            var model = new AtencionVeterinariaViewModel
            {
                Id = atencion.Id,
                HistoriaClinicaId = atencion.HistoriaClinicaId,
                VeterinarioId = atencion.VeterinarioId,
                Fecha = atencion.Fecha,
                Diagnostico = atencion.Diagnostico,
                PesoKg = atencion.PesoMascota,

                // Mapeo del tratamiento
                Medicamento = atencion.Tratamiento?.Medicamento,
                Dosis = atencion.Tratamiento?.Dosis,
                Frecuencia = atencion.Tratamiento?.Frecuencia,
                DuracionDias = atencion.Tratamiento?.Duracion, 
                ObservacionesTratamiento = atencion.Tratamiento?.Observaciones,

                VacunasSeleccionadasIds = atencion.Vacunas?.Select(v => v.Id).ToList() ?? new List<int>(),
                EstudiosSeleccionadosIds = atencion.EstudiosComplementarios?.Select(e => e.Id).ToList() ?? new List<int>()
            };

            var vacunas = await _vacunaService.ListarTodo();
            var estudios = await _estudioService.ListarTodo();
            model.VacunasDisponibles = new SelectList(vacunas, "Id", "Nombre");
            model.EstudiosDisponibles = new SelectList(estudios, "Id", "Nombre");

            return model;
        }
        // Metodo para restaurar (Volver al pasado)
        public async Task RestaurarVersionAsync(int mementoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Buscar la copia de seguridad
                var memento = await _context.AtencionMementos.FindAsync(mementoId);
                if (memento == null) throw new Exception("Versión histórica no encontrada");

                // Buscar la entidad real actual
                var atencionActual = await _context.AtencionesVeterinarias
                    .Include(a => a.Tratamiento)
                    .FirstOrDefaultAsync(a => a.Id == memento.AtencionVeterinariaId);

                if (atencionActual == null) throw new Exception("La atención original ya no existe");


                // Sobrescribir (Restaurar datos)
                atencionActual.Diagnostico = memento.Diagnostico;
                atencionActual.PesoMascota = memento.PesoMascota;

                // Restaurar Tratamiento
                if (atencionActual.Tratamiento != null)
                {
                    // Si tiene tratamiento, pisamos los datos con los del memento
                    atencionActual.Tratamiento.Medicamento = memento.TratamientoMedicamento;
                    atencionActual.Tratamiento.Dosis = memento.TratamientoDosis;
                    atencionActual.Tratamiento.Frecuencia = memento.TratamientoFrecuencia;
                    atencionActual.Tratamiento.Duracion = memento.TratamientoDuracion;
                    atencionActual.Tratamiento.Observaciones = memento.TratamientoObservaciones;
                }
                else if (!string.IsNullOrEmpty(memento.TratamientoMedicamento))
                {
                    // Si la versión vieja tenía tratamiento y la actual no, lo recreamos
                    var tratamientoRestaurado = new Tratamiento
                    {
                        Medicamento = memento.TratamientoMedicamento,
                        Dosis = memento.TratamientoDosis,
                        Frecuencia = memento.TratamientoFrecuencia,
                        Duracion = memento.TratamientoDuracion,
                        Observaciones = memento.TratamientoObservaciones
                    };
                    _context.Tratamientos.Add(tratamientoRestaurado);
                    atencionActual.Tratamiento = tratamientoRestaurado;
                }

                _context.AtencionesVeterinarias.Update(atencionActual);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Metodo para ver la lista
        public async Task<List<AtencionVeterinariaMemento>> ObtenerHistorialAsync(int atencionId)
        {
            return await _context.AtencionMementos
                .Where(m => m.AtencionVeterinariaId == atencionId)
                .OrderByDescending(m => m.FechaVersion) // Las más recientes primero
                .ToListAsync();
        }
        #endregion
        public async Task<decimal> SumarCostosAtencionesMesActualAsync()
        {
            return await _repository.SumarCostosAtencionesMesActualAsync();
        }

        public async Task<(string Nombre, string Lote)> ObtenerVacunaMasFrecuenteAsync()
        {
            return await _repository.ObtenerVacunaMasFrecuenteAsync();
        }

        public async Task<(string Nombre, decimal Precio)> ObtenerEstudioMasSolicitadoAsync()
        {
            return await _repository.ObtenerEstudioMasSolicitadoAsync();
        }

        public async Task<int> CantidadAtencionesPorVeterinario(int id)
        {
            return await _repository.CantidadAtencionesPorVeterinario(id);
        }

        public async Task<Mascota> ObtenerMascotaMasFrecuentePorVeterinario(int idVeterinario)
        {
            return await _repository.ObtenerMascotaMasFrecuentePorVeterinario(idVeterinario);
        }

        public async Task<int> CantidadAtenciones()
        {
            return await _repository.CantidadAtenciones();
        }

        public async Task<int> CantidadPagosPendientes(int idCliente)
        {
            return await _repository.CantidadPagosPendientes(idCliente);
        }

        public async Task<Cliente> ObtenerClienteMasFrecuenteAsync()
        {
            return await _repository.ObtenerClienteMasFrecuenteAsync();
        }

        public async Task<decimal> SumarIngresosAsync()
        {
            return await _repository.SumarIngresosAsync();
        }
        public async Task<List<DashboardViewModel.IngresosAnualesData>> ObtenerDatosIngresosAnualesAsync(List<int> anios)
        {
            return await _repository.ObtenerDatosIngresosAnualesAsync(anios);
        }

        public async Task<List<DashboardViewModel.ServicioCountData>> ContarTopServiciosAsync(int topN)
        {
            return await _repository.ContarTopServiciosAsync(topN);
        }
        
        public async Task<List<DashboardViewModel.IngresosMensualesData>> ObtenerDatosIngresosMensualesAsync(int anio)
        {
            return await _repository.ObtenerDatosIngresosMensualesAsync(anio);
        }
        public async Task<List<DashboardViewModel.AtencionesPorVeterinarioData>> ContarAtencionesPorVeterinarioAsync(DateTime? inicio, DateTime? fin)
        {
            return await _repository.ContarAtencionesPorVeterinarioAsync(inicio,fin);
        }

        public async Task<List<AtencionVeterinaria>> ObtenerAtencionesPorMesAsync(int anio, int mes)
        {
            return await _repository.ObtenerAtencionesPorMesAsync(anio,mes);
        }

        public async Task<List<AtencionVeterinaria>> ObtenerAtencionesPorIdCliente(List<int> ids)
        {
            return await _repository.ObtenerAtencionesPorIdCliente(ids);
        }

        public async Task ActualizarAtencionesAsync(List<AtencionVeterinaria> atenciones)
        {
            await _repository.ActualizarAtencionesAsync(atenciones);
        }
        
        public async Task ActualizarAtencionAsync(AtencionVeterinaria atencion)
        {
            await _repository.ActualizarAtencionAsync(atencion);
        }
    }
}
