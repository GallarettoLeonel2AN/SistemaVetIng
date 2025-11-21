namespace SistemaVetIng.Models.Singleton
{
    public interface IConfiguracionHorarioCache
    {
        ConfiguracionVeterinaria Configuracion { get; }
        void SetConfiguracion(ConfiguracionVeterinaria config);
    }
}
