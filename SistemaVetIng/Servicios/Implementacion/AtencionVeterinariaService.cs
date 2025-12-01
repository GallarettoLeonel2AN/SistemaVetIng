using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Decorator;
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

            ICostoAtencion calculadorCosto = new CostoBaseAtencion(costoConsultaBase, costoVacunas, costoEstudios);
            calculadorCosto = new RecargoFinDeSemana(calculadorCosto);

            var historiaClinica = await _historiaClinicaService.GetHistoriaClinicaConMascotayPropietario(model.HistoriaClinicaId);

            if (historiaClinica != null)
            {
                // Recargo por Raza Peligrosa
                if (historiaClinica.Mascota.RazaPeligrosa)
                {
                    calculadorCosto = new RecargoRazaPeligrosa(calculadorCosto);
                }

                // Descuento por Cliente Frecuente
                if (historiaClinica.Mascota.Propietario != null)
                {
                    int clienteId = historiaClinica.Mascota.Propietario.Id;

                    int cantidadVisitas = await _repository.ContarAtencionesHistoricasPorCliente(clienteId);

                    // Si vino más de 3 veces, aplicamos descuento
                    if (cantidadVisitas > 3)
                    {
                        calculadorCosto = new DescuentoClienteFrecuente(calculadorCosto);
                    }
                }
            }
           
            decimal costoTotalFinal = calculadorCosto.Calcular();

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
                CostoTotal = costoTotalFinal,
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
                decimal costoConsultaBase = 5000;
                decimal costoTotal = costoVacunas + costoConsultaBase + costoEstudios;

                ICostoAtencion calculadorCosto = new CostoBaseAtencion(costoConsultaBase, costoVacunas, costoEstudios);
                calculadorCosto = new RecargoFinDeSemana(calculadorCosto);

                var historiaClinica = await _historiaClinicaService.GetHistoriaClinicaConMascotayPropietario(model.HistoriaClinicaId);

                if (historiaClinica != null)
                {
                    // Recargo por Raza Peligrosa
                    if (historiaClinica.Mascota.RazaPeligrosa)
                    {
                        calculadorCosto = new RecargoRazaPeligrosa(calculadorCosto);
                    }

                    // Descuento por Cliente Frecuente
                    if (historiaClinica.Mascota.Propietario != null)
                    {
                        int clienteId = historiaClinica.Mascota.Propietario.Id;

                        int cantidadVisitas = await _repository.ContarAtencionesHistoricasPorCliente(clienteId);

                        // Si vino más de 3 veces, aplicamos descuento
                        if (cantidadVisitas > 3)
                        {
                            calculadorCosto = new DescuentoClienteFrecuente(calculadorCosto);
                        }
                    }
                }

                decimal costoTotalFinal = calculadorCosto.Calcular();


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
                    CostoTotal = costoTotalFinal,
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
                // CARGAR DATOS COMPLETOS
                var atencionActual = await _context.AtencionesVeterinarias
                    .Include(a => a.Tratamiento)
                    .Include(a => a.Vacunas)
                    .Include(a => a.EstudiosComplementarios)
                    .Include(a => a.HistoriaClinica)
                        .ThenInclude(hc => hc.Mascota)
                    .Include(a => a.HistoriaClinica.Mascota.Propietario)
                    .FirstOrDefaultAsync(a => a.Id == model.Id);

                if (atencionActual == null) throw new Exception($"No se encontró la atención {model.Id}");

                // CREAR MEMENTO (Respaldo)
                var memento = new AtencionVeterinariaMemento
                {
                    AtencionVeterinariaId = atencionActual.Id,
                    FechaVersion = DateTime.Now,
                    UsuarioEditor = user.Identity?.Name ?? "Desconocido",
                    MotivoCambio = motivo,
                    Diagnostico = atencionActual.Diagnostico,
                    PesoMascota = atencionActual.PesoMascota,
                    // Datos del tratamiento viejo (puede ser null)
                    TratamientoMedicamento = atencionActual.Tratamiento?.Medicamento,
                    TratamientoDosis = atencionActual.Tratamiento?.Dosis,
                    TratamientoFrecuencia = atencionActual.Tratamiento?.Frecuencia,
                    TratamientoDuracion = atencionActual.Tratamiento?.Duracion,
                    TratamientoObservaciones = atencionActual.Tratamiento?.Observaciones,
                    // Snapshots de listas
                    VacunasSnapshot = string.Join(", ", atencionActual.Vacunas.Select(v => v.Nombre)),
                    EstudiosSnapshot = string.Join(", ", atencionActual.EstudiosComplementarios.Select(e => e.Nombre))
                };

                _context.AtencionMementos.Add(memento);
                await _context.SaveChangesAsync(); 

                // ACTUALIZAR DATOS BÁSICOS
                atencionActual.Diagnostico = model.Diagnostico;
                atencionActual.PesoMascota = model.PesoKg;

                // ACTUALIZAR O CREAR TRATAMIENTO
                // Validamos si el usuario ingresó algo en el tratamiento
                bool hayDatosTratamiento = !string.IsNullOrWhiteSpace(model.Medicamento);

                if (atencionActual.Tratamiento != null)
                {
                    // Si ya existía, actualizamos
                    atencionActual.Tratamiento.Medicamento = model.Medicamento;
                    atencionActual.Tratamiento.Dosis = model.Dosis;
                    atencionActual.Tratamiento.Frecuencia = model.Frecuencia;
                    atencionActual.Tratamiento.Duracion = model.DuracionDias;
                    atencionActual.Tratamiento.Observaciones = model.ObservacionesTratamiento;
                }
                else if (hayDatosTratamiento)
                {
                    // Si NO existía y ahora pusieron datos, CREAMOS UNO NUEVO
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

                // ACTUALIZAR LISTAS (VACUNAS / ESTUDIOS)
                // Limpiamos relaciones actuales
                atencionActual.Vacunas.Clear();
                atencionActual.EstudiosComplementarios.Clear();

                // Agregamos nuevas vacunas si hay seleccionadas
                List<Vacuna> vacunasNuevas = new List<Vacuna>();
                if (model.VacunasSeleccionadasIds != null && model.VacunasSeleccionadasIds.Any())
                {
                    vacunasNuevas = await _context.Vacunas
                        .Where(v => model.VacunasSeleccionadasIds.Contains(v.Id))
                        .ToListAsync();
                    foreach (var v in vacunasNuevas) atencionActual.Vacunas.Add(v);
                }

                // Agregamos nuevos estudios si hay seleccionados
                List<Estudio> estudiosNuevos = new List<Estudio>();
                if (model.EstudiosSeleccionadosIds != null && model.EstudiosSeleccionadosIds.Any())
                {
                    estudiosNuevos = await _context.Estudios
                        .Where(e => model.EstudiosSeleccionadosIds.Contains(e.Id))
                        .ToListAsync();
                    foreach (var e in estudiosNuevos) atencionActual.EstudiosComplementarios.Add(e);
                }


                decimal costoConsultaBase = 5000; 
                decimal costoVacunas = vacunasNuevas.Sum(v => v.Precio);
                decimal costoEstudios = estudiosNuevos.Sum(e => e.Precio);

                ICostoAtencion calculador = new CostoBaseAtencion(costoConsultaBase, costoVacunas, costoEstudios);

                // Decorators
                calculador = new RecargoFinDeSemana(calculador);
                if (atencionActual.HistoriaClinica.Mascota.RazaPeligrosa)
                {
                    calculador = new RecargoRazaPeligrosa(calculador);
                }

                // Lógica Cliente Frecuente
                if (atencionActual.HistoriaClinica.Mascota.Propietario != null)
                {
                    int clienteId = atencionActual.HistoriaClinica.Mascota.Propietario.Id;
                    int cantidadVisitas = await _repository.ContarAtencionesHistoricasPorCliente(clienteId);

                    if (cantidadVisitas > 50)
                    {
                        calculador = new DescuentoClienteFrecuente(calculador);
                    }
                }

                atencionActual.CostoTotal = calculador.Calcular();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error al guardar en BD: {(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}");
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

        // Metodo para restaurar la version
        public async Task RestaurarVersionAsync(int mementoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //  Buscar el memento
                var memento = await _context.AtencionMementos.FindAsync(mementoId);
                if (memento == null) throw new Exception("Versión histórica no encontrada");

                //  Buscar atencion actual 
                var atencionActual = await _context.AtencionesVeterinarias
                    .Include(a => a.Tratamiento)
                    .Include(a => a.Vacunas)
                    .Include(a => a.EstudiosComplementarios)
                    .Include(a => a.HistoriaClinica)
                        .ThenInclude(hc => hc.Mascota)
                    .Include(a => a.HistoriaClinica.Mascota.Propietario)
                    .FirstOrDefaultAsync(a => a.Id == memento.AtencionVeterinariaId);

                if (atencionActual == null) throw new Exception("La atención original ya no existe");

              
                atencionActual.Diagnostico = memento.Diagnostico;
                atencionActual.PesoMascota = memento.PesoMascota;

                //  Restaurar Tratamiento
                if (atencionActual.Tratamiento != null)
                {
                    atencionActual.Tratamiento.Medicamento = memento.TratamientoMedicamento;
                    atencionActual.Tratamiento.Dosis = memento.TratamientoDosis;
                    atencionActual.Tratamiento.Frecuencia = memento.TratamientoFrecuencia;
                    atencionActual.Tratamiento.Duracion = memento.TratamientoDuracion;
                    atencionActual.Tratamiento.Observaciones = memento.TratamientoObservaciones;
                    _context.Entry(atencionActual.Tratamiento).State = EntityState.Modified;
                }
                else if (!string.IsNullOrEmpty(memento.TratamientoMedicamento))
                {
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

                //  Restaurar Vacunas
                atencionActual.Vacunas.Clear();
                if (!string.IsNullOrEmpty(memento.VacunasSnapshot))
                {
                    var nombresVacunas = memento.VacunasSnapshot.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    var vacunasRestauradas = await _context.Vacunas
                        .Where(v => nombresVacunas.Contains(v.Nombre))
                        .ToListAsync();
                    foreach (var v in vacunasRestauradas) atencionActual.Vacunas.Add(v);
                }

                // Restaurar Estudios
                atencionActual.EstudiosComplementarios.Clear();
                if (!string.IsNullOrEmpty(memento.EstudiosSnapshot))
                {
                    var nombresEstudios = memento.EstudiosSnapshot.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    var estudiosRestaurados = await _context.Estudios
                        .Where(e => nombresEstudios.Contains(e.Nombre))
                        .ToListAsync();
                    foreach (var e in estudiosRestaurados) atencionActual.EstudiosComplementarios.Add(e);
                }

                // recalcular precios con decorator
               
                decimal costoConsultaBase = 5000;
              
                decimal costoVacunas = atencionActual.Vacunas.Sum(v => v.Precio);
                decimal costoEstudios = atencionActual.EstudiosComplementarios.Sum(e => e.Precio);

               
                ICostoAtencion calculador = new CostoBaseAtencion(costoConsultaBase, costoVacunas, costoEstudios);

            
                calculador = new RecargoFinDeSemana(calculador);

                //  Raza Peligrosa
                if (atencionActual.HistoriaClinica.Mascota.RazaPeligrosa)
                {
                    calculador = new RecargoRazaPeligrosa(calculador);
                }

                // Cliente Frecuente
                if (atencionActual.HistoriaClinica.Mascota.Propietario != null)
                {
                    int clienteId = atencionActual.HistoriaClinica.Mascota.Propietario.Id;
                    int cantidadVisitas = await _repository.ContarAtencionesHistoricasPorCliente(clienteId);

                    if (cantidadVisitas > 50)
                    {
                        calculador = new DescuentoClienteFrecuente(calculador);
                    }
                }

              
                atencionActual.CostoTotal = calculador.Calcular();
                

                _context.Entry(atencionActual).State = EntityState.Modified;
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
