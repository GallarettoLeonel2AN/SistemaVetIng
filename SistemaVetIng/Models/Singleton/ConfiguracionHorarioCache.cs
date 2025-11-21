namespace SistemaVetIng.Models.Singleton
{
    public sealed class ConfiguracionHorarioCache : IConfiguracionHorarioCache
    {
        private static readonly Lazy<ConfiguracionHorarioCache> _instancia =
            new(() => new ConfiguracionHorarioCache());

        public static ConfiguracionHorarioCache Instancia => _instancia.Value;

        // La config cargada desde la BD
        public ConfiguracionVeterinaria Configuracion { get; private set; }

        private ConfiguracionHorarioCache() { }

        // Metodo para actualizar los datos cacheados
        public void SetConfiguracion(ConfiguracionVeterinaria config)
        {
            Configuracion = config;
        }
    }
}
