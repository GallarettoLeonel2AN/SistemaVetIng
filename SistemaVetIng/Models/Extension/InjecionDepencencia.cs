using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemaVetIng.Data;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Models.Extension
{
    public static class InjecionDepencencia
    {
        public static IServiceCollection AddDatabase(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGeneralRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IVeterinarioRepository, VeterinarioRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IMascotaRepository, MascotaRepository>();
            services.AddScoped<IGeneralRepository<Chip>, ChipRepository>();
            services.AddScoped<IGeneralRepository<ConfiguracionVeterinaria>, VeterinariaConfiguracionRepository>();
            services.AddScoped<IHistoriaClinicaRepository, HistoriaClinicaRepository>();
            services.AddScoped<IAtencionVeterinariaRepository, AtencionVeterinariaRepository>();
            services.AddScoped<ITurnoRepository, TurnoRepository>();
            services.AddScoped<IConfiguracionVeterinariaRepository, ConfiguracionVeterinariaRepository>();
            services.AddScoped<IVacunaRepository, VacunaRepository>();
            services.AddScoped<IEstudioRepository, EstudioRepository>();

            return services;
        }


        public static IServiceCollection AddServices(this IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<SmtpSettings>>().Value);
            services.AddScoped<IVeterinarioService, VeterinarioService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IMascotaService, MascotaService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IVeterinariaConfigService, VeterinariaConfigService>();
            services.AddScoped<IVeterinariaService, VeterinariaService>();
            services.AddScoped<IHistoriaClinicaService, HistoriaClinicaService>();
            services.AddScoped<IAtencionVeterinariaService, AtencionVeterinariaService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITurnoService, TurnoService>();
            services.AddScoped<IVacunaService, VacunaService>();
            services.AddScoped<IEstudioService, EstudioService>();
            services.AddScoped<IMercadoPagoService, MercadoPagoService>();

            return services;
        }
    }
}
