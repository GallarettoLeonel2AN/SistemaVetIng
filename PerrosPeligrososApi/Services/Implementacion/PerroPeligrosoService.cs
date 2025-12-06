using Microsoft.EntityFrameworkCore;
using PerrosPeligrososApi.Data;
using PerrosPeligrososApi.Models;
using PerrosPeligrososApi.Models.Dtos;
using PerrosPeligrososApi.Services.Interface;
using static PerrosPeligrososApi.Models.Dtos.PerroPeligrosoResponseDto;

namespace PerrosPeligrososApi.Services.Implementacion
{
    public class PerroPeligrosoService : IPerroPeligrosoService
    {
        private readonly PerrosPeligrososApiDbContext _context;
        private readonly ILogger<PerroPeligrosoService> _logger;

        public PerroPeligrosoService(
            PerrosPeligrososApiDbContext context,
            ILogger<PerroPeligrosoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> ProcesarRegistro(PerroPeligrosoRegistroDto registroDto)
        {
            // Lógica de Update or Insert

            // Buscamos si ya existe por el ID original de Veting
            var perroExistente = await _context.PerrosPeligrosos
                .Include(p => p.Chip)
                .FirstOrDefaultAsync(p => p.MascotaIdOriginal == registroDto.MascotaId);

            PerroPeligroso perroEntidad;

            if (perroExistente != null)
            {
                // --- ACTUALIZAR ---
                _logger.LogInformation($"Actualizando perro ID Original: {registroDto.MascotaId}");

                perroExistente.Nombre = registroDto.NombreMascota;
                perroExistente.Raza = registroDto.RazaMascota;
                perroExistente.ClienteDni = registroDto.ClienteDni;
                perroExistente.ClienteNombre = registroDto.ClienteNombre;
                perroExistente.ClienteApellido = registroDto.ClienteApellido;
                perroExistente.FechaRegistroApi = DateTime.Now;

                // Lógica del Chip (Actualizar o Borrar)
                if (registroDto.TieneChip && !string.IsNullOrEmpty(registroDto.ChipCodigo))
                {
                    if (perroExistente.Chip != null)
                    {
                        perroExistente.Chip.Codigo = registroDto.ChipCodigo;
                    }
                    else
                    {
                        perroExistente.Chip = new ChipPerroPeligroso
                        {
                            Codigo = registroDto.ChipCodigo
                        };
                    }
                }
                else
                {
                    // Si viene sin chip, y tenía uno, se borra
                    if (perroExistente.Chip != null)
                    {
                        _context.ChipsPerroPeligroso.Remove(perroExistente.Chip);
                        perroExistente.Chip = null;
                    }
                }

                perroEntidad = perroExistente;
            }
            else
            {
                // --- CREAR NUEVO ---
                _logger.LogInformation($"Creando nuevo perro ID Original: {registroDto.MascotaId}");

                perroEntidad = new PerroPeligroso
                {
                    Nombre = registroDto.NombreMascota,
                    Raza = registroDto.RazaMascota,
                    MascotaIdOriginal = registroDto.MascotaId,
                    ClienteDni = registroDto.ClienteDni,
                    ClienteNombre = registroDto.ClienteNombre,
                    ClienteApellido = registroDto.ClienteApellido,
                    FechaRegistroApi = DateTime.Now
                };

                if (registroDto.TieneChip && !string.IsNullOrEmpty(registroDto.ChipCodigo))
                {
                    perroEntidad.Chip = new ChipPerroPeligroso
                    {
                        Codigo = registroDto.ChipCodigo
                    };
                }

                _context.PerrosPeligrosos.Add(perroEntidad);
            }

            // Guardamos cambios
            await _context.SaveChangesAsync();

            return perroEntidad.Id;
        }

        public async Task<IEnumerable<PerroPeligrosoResponseDto>> ObtenerTodos()
        {
            var lista = await _context.PerrosPeligrosos
                    .AsNoTracking()
                    .Select(p => new PerroPeligrosoResponseDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Raza = p.Raza,
                        MascotaIdOriginal = p.MascotaIdOriginal,
                        ClienteDni = p.ClienteDni,
                        ClienteNombre = p.ClienteNombre,
                        ClienteApellido = p.ClienteApellido,
                        FechaRegistroApi = p.FechaRegistroApi,

                        // Mapeo manual del chip (si existe)
                        Chip = p.Chip != null ? new ChipResponseDto
                        {
                            Codigo = p.Chip.Codigo
                        } : null
                    })
                    .ToListAsync();

            return lista;
        }
        public async Task<PerroPeligrosoResponseDto> ObtenerPorId(int id)
        {
            var perro = await _context.PerrosPeligrosos
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PerroPeligrosoResponseDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Raza = p.Raza,
                    MascotaIdOriginal = p.MascotaIdOriginal,
                    ClienteDni = p.ClienteDni,
                    ClienteNombre = p.ClienteNombre,
                    ClienteApellido = p.ClienteApellido,
                    FechaRegistroApi = p.FechaRegistroApi,
                    Chip = p.Chip != null ? new ChipResponseDto
                    {
                        Codigo = p.Chip.Codigo
                    } : null
                })
                .FirstOrDefaultAsync();

            return perro;
        }

        public async Task<IEnumerable<PerroPeligrosoResponseDto>> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino)) return new List<PerroPeligrosoResponseDto>();

            termino = termino.ToLower().Trim();

            var lista = await _context.PerrosPeligrosos
                .AsNoTracking()
                .Where(p =>
                    p.ClienteDni.ToString().Contains(termino) || // Busca por DNI
                    (p.Chip != null && p.Chip.Codigo.ToLower().Contains(termino)) // Busca por Chip
                )
                .Select(p => new PerroPeligrosoResponseDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Raza = p.Raza,
                    MascotaIdOriginal = p.MascotaIdOriginal,
                    ClienteDni = p.ClienteDni,
                    ClienteNombre = p.ClienteNombre,
                    ClienteApellido = p.ClienteApellido,
                    FechaRegistroApi = p.FechaRegistroApi,
                    Chip = p.Chip != null ? new ChipResponseDto
                    {
                        Codigo = p.Chip.Codigo
                    } : null
                })
                .ToListAsync();

            return lista;
        }

    }
}
