using MercadoPago.Config;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Extensions;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Extension;
using SistemaVetIng.Models.Indentity;


var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

// Base de datos
if (environment == "Testing")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TestDB"));
}
else
{
    // Configuración de conexion
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
}



// MErcado Pago
var mpSettings = builder.Configuration.GetSection("MercadoPagoSettings").Get<MercadoPagoSettings>();
if (mpSettings != null && !string.IsNullOrEmpty(mpSettings.AccessToken))
{
    MercadoPagoConfig.AccessToken = mpSettings.AccessToken;
}
else
{
    // Manejo de error si falta la configuración
    throw new InvalidOperationException("Falta o no está configurado el AccessToken de Mercado Pago.");
}


builder.Services.AddIdentity<Usuario, Rol>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddAppAuthorization(); 



// Configuración explícita de la cookie de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Duración de la sesión
    options.SlidingExpiration = true; // Renueva la sesión con la actividad
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1); // Tiempo de inactividad
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Inyección del Repositorio 
builder.Services.AddRepositories()
    .AddServices();

// AddNToastNotifyToastr
builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions()
    {
        // Posición
        PositionClass = ToastPositions.TopRight,

        // Botón de cierre y barra de progreso
        CloseButton = true,
        ProgressBar = true,

        // Duración y comportamiento
        TimeOut = 5000,
        ExtendedTimeOut = 1000,
        NewestOnTop = true,
        TapToDismiss = false // La notificación no se cierra al hacer clic
    });

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Seeder de Identity
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAndAdminAsync(services);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.UseNToastNotify();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

public partial class Program { } // Para test de integracion