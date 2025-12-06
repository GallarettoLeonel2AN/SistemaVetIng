using Microsoft.EntityFrameworkCore;
using PerrosPeligrososApi.Data;
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
builder.Services.AddSwaggerGen();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PerrosPeligrososApiDbContext>(options =>
    options.UseSqlServer(connectionString));


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();


app.UseCors("AllowAllOrigins"); 

app.MapControllers();

app.Run();