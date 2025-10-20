using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class TurnoService : ITurnoService
    {
        private readonly IVeterinariaConfigService _veterinariaService;
        private readonly ITurnoRepository _turnoRepository;

        public TurnoService(IVeterinariaConfigService veterinariaService, ITurnoRepository turnoRepository)
        {
            _veterinariaService = veterinariaService;
            _turnoRepository = turnoRepository;
        }

        public async Task<List<string>> GetHorariosDisponiblesAsync(DateTime fecha)
        {
            var configuracion = await _veterinariaService.ObtenerConfiguracionAsync();
            if (configuracion == null || configuracion.HorariosPorDia == null)
            {
                return new List<string>();
            }

            try
            {
                var horariosPosibles = GenerarHorarios(configuracion, fecha);
                var turnosOcupados = (await _turnoRepository.GetTurnosByFecha(fecha))
                    .Select(t => t.Horario.ToString(@"hh\:mm"))
                    .ToHashSet();
                var horariosDisponibles = horariosPosibles.Where(h => !turnosOcupados.Contains(h)).ToList();
                return horariosDisponibles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener horarios disponibles: {ex.Message}");
                return new List<string>();
            }
        }


        public async Task ReservarTurnoAsync(ReservaTurnoViewModel model)
        {
            var turno = new Turno
            {
                Fecha = model.Fecha.Date,
                Horario = model.Horario,
                Motivo = model.Motivo,
                MascotaId = model.MascotaId,
                ClienteId = model.ClienteId,
                Estado = "Pendiente",
                PrimeraCita = model.PrimeraCita
            };

            await _turnoRepository.AgregarTurno(turno);
            await _turnoRepository.Guardar();
        }

        private List<string> GenerarHorarios(ConfiguracionVeterinaria config, DateTime fecha)
        {
            var horarios = new List<string>();
            var diaDeLaSemana = fecha.DayOfWeek;
            var horarioDelDia = config.HorariosPorDia.FirstOrDefault(h => h.DiaSemana == diaDeLaSemana);

            if (horarioDelDia == null || !horarioDelDia.EstaActivo || !horarioDelDia.HoraInicio.HasValue || !horarioDelDia.HoraFin.HasValue)
            {
                return horarios;
            }

            var horaActual = horarioDelDia.HoraInicio.Value;
            var horaFin = horarioDelDia.HoraFin.Value;
            var duracion = config.DuracionMinutosPorConsulta;

            while (horaActual < horaFin)
            {
                horarios.Add(horaActual.ToString("HH:mm"));
                horaActual = horaActual.AddMinutes(duracion);
            }

            return horarios;
        }


        public async Task<IEnumerable<Turno>> ObtenerTurnosAsync()
        {
            return await _turnoRepository.ListarTodo();
        }

        public async Task<IEnumerable<Turno>> ObtenerTurnosPorClienteIdAsync(int clienteId)
        {
            return await _turnoRepository.ObtenerTurnosPorClienteIdAsync(clienteId);
        }

        public async Task<IEnumerable<Turno>> ObtenerTurnosPorFechaAsync(DateTime fecha)
        {
            return await _turnoRepository.ObtenerTurnosPorFechaAsync(fecha);
        }
        public async Task<Turno> ObtenerPorIdConDatosAsync(int id)
        {
            return await _turnoRepository.ObtenerPorIdConDatosAsync(id);
        }

        public void Actualizar(Turno turno)
        {
           _turnoRepository.Actualizar(turno);
        }

        public async Task Guardar()
        {
            await _turnoRepository.Guardar();
        }

        public async Task<(bool success, string message)> CancelarTurnoAsync(int turnoId, ClaimsPrincipal user)
        {
            var turno = await _turnoRepository.ObtenerPorIdConDatosAsync(turnoId); 

            if (turno == null)
            {
                return (false, "El turno que intentas cancelar no existe.");
            }

            // Solo se pueden cancelar turnos pendientes.
            if (turno.Estado != "Pendiente")
            {
                return (false, $"No se puede cancelar un turno que está en estado '{turno.Estado}'.");
            }

            // Validación de Permiso 
            var usuarioIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(usuarioIdString, out int usuarioId);

            // Si el usuario es un Cliente, verifica que sea SU turno.
            if (user.IsInRole("Cliente") && turno.Cliente?.UsuarioId != usuarioId)
            {
                return (false, "No tienes permiso para cancelar este turno.");
            }

            // Cambio de estado
            turno.Estado = "Cancelado";

            try
            {
                Actualizar(turno);
                await _turnoRepository.Guardar();
                return (true, "Turno cancelado con éxito.");
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar cancelar el turno.");
            }
        }
    }
}