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
using SistemaVetIng.Models.Observer;
using SistemaVetIng.Models.Singleton;

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
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Mercado Pago
var mpSettings = builder.Configuration.GetSection("MercadoPagoSettings").Get<MercadoPagoSettings>();
if (mpSettings != null && !string.IsNullOrEmpty(mpSettings.AccessToken))
{
    MercadoPagoConfig.AccessToken = mpSettings.AccessToken;
}
else
{
    throw new InvalidOperationException("Falta o no está configurado el AccessToken de Mercado Pago.");
}

builder.Services.AddIdentity<Usuario, Rol>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Email sender
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddAppAuthorization();

// Patron Singleton
builder.Services.AddSingleton<IConfiguracionHorarioCache>(sp =>
    ConfiguracionHorarioCache.Instancia
);

// Patron Observer
builder.Services.AddScoped<IClienteObserver, BienvenidaEmailObserver>();
builder.Services.AddScoped<IClienteSubject, ClienteSubject>();


// Cookie auth
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Repositorios y servicios
builder.Services.AddRepositories()
    .AddServices();

builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions()
    {
        PositionClass = ToastPositions.TopRight,
        CloseButton = true,
        ProgressBar = true,
        TimeOut = 5000,
        ExtendedTimeOut = 1000,
        NewestOnTop = true,
        TapToDismiss = false
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

if (!app.Environment.IsEnvironment("Testing"))
{
    // Seeder de Identity
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await IdentitySeeder.SeedRolesAndAdminAsync(services, builder.Configuration);
    }
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

public partial class Program { }
