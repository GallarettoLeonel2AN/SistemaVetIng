using SistemaVetIng.Models;
using SistemaVetIng.Models.Singleton;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;


namespace SistemaVetIng.Servicios.Implementacion
{
    public class VeterinariaConfigService : IVeterinariaConfigService
    {
        private readonly IConfiguracionVeterinariaRepository _configRepository;
        private readonly IVeterinariaRepository _veterinariaRepository;
        private readonly IConfiguracionHorarioCache _cacheInstance;

        public VeterinariaConfigService(
            IConfiguracionVeterinariaRepository configRepository,
            IVeterinariaRepository veterinariaRepository,
            IConfiguracionHorarioCache cacheInstance)
        {
            _configRepository = configRepository;
            _veterinariaRepository = veterinariaRepository;
            _cacheInstance = cacheInstance;
        }

        public async Task<ConfiguracionVeterinaria> ObtenerConfiguracionAsync()
        {
            var config = await _configRepository.ObtenerConfiguracionConHorariosAsync();

            // Si el Singleton no tiene datos, lo cargamos
            if (_cacheInstance.Configuracion == null && config != null)
                _cacheInstance.SetConfiguracion(config);

            return config;
        }

        public async Task<ConfiguracionVeterinaria> Guardar(ConfiguracionVeterinaria model)
        {
            try
            {
                var configExistente = await _configRepository.ObtenerConfiguracionConHorariosAsync();

                if (configExistente == null)
                {
                  
                    await _configRepository.AgregarAsync(model);


                    // Vinculamos a Veterinaria
                    var veterinaria = await _veterinariaRepository.ObtenerPrimeraAsync();
                    veterinaria.ConfiguracionVeterinaria = model;
                    await _veterinariaRepository.Guardar();
                }
                else
                {
                    // Si ya existe, actualizamos sus propiedades.
                    configExistente.DuracionMinutosPorConsulta = model.DuracionMinutosPorConsulta;

                    // Actualizamos los horarios uno por uno.
                    foreach (var horarioNuevo in model.HorariosPorDia)
                    {
                        var horarioExistente = configExistente.HorariosPorDia
                            .FirstOrDefault(h => h.DiaSemana == horarioNuevo.DiaSemana);

                        if (horarioExistente != null)
                        {
                            horarioExistente.EstaActivo = horarioNuevo.EstaActivo;
                            horarioExistente.HoraInicio = horarioNuevo.HoraInicio;
                            horarioExistente.HoraFin = horarioNuevo.HoraFin;
                        }
                    }

                    _configRepository.Actualizar(configExistente);
                }

                await _configRepository.GuardarCambiosAsync();

                // Actualizamos Singleton
                _cacheInstance.SetConfiguracion(model);

                return model;
            }
            catch (Exception ex)
            {

                throw new Exception("No se pudo guardar la configuración.", ex);
            }
        }
    }
}
