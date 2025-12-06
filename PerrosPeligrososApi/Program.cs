using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PerrosPeligrososApi.Data;
using PerrosPeligrososApi.Middleware;
using PerrosPeligrososApi.Services.Implementacion;
using PerrosPeligrososApi.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddScoped<IPerroPeligrosoService, PerroPeligrosoService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin() // Permite peticiones desde cualquier origen
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gobierno Perros Peligrosos", Version = "v1" });

    // Definimos el esquema de seguridad (API Key)
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Ingresa la API Key",
        Name = "PERROPELIGROSO-API-KEY", 
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    // Establecer Swagger esquema global
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new List<string>()
        }
    });
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PerrosPeligrososApiDbContext>(options =>
    options.UseSqlServer(connectionString));


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Authorized
app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();


app.UseCors("AllowAllOrigins"); 

app.MapControllers();

app.Run();