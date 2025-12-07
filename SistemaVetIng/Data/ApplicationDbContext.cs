using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Models.Memento;
using System.Diagnostics;

namespace SistemaVetIng.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {}


        // DbSets de cada entidad
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Veterinario> Veterinarios { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Chip> Chips { get; set; }
        public DbSet<HistoriaClinica> HistoriasClinicas { get; set; }
        public DbSet<AtencionVeterinaria> AtencionesVeterinarias { get; set; }
        public DbSet<Tratamiento> Tratamientos { get; set; }
        public DbSet<Vacuna> Vacunas { get; set; }
        public DbSet<Estudio> Estudios { get; set; }
        public DbSet<ConfiguracionVeterinaria> ConfiguracionVeterinarias { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Veterinaria> Veterinarias { get; set; }
        public DbSet<AuditoriaEvento> AuditoriaEventos { get; set; }
        public DbSet<AtencionVeterinariaMemento> AtencionMementos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

          
            // Seeds

            modelBuilder.Entity<Estudio>().HasData(
                new Estudio { Id = 1, Nombre = "Análisis de sangre completo", Precio = 4500.00m},
                new Estudio { Id = 2, Nombre = "Radiografía de tórax", Precio = 6000.00m },
                new Estudio { Id = 3, Nombre = "Análisis de orina", Precio = 2000.00m },
                new Estudio { Id = 4, Nombre = "Ecografía abdominal", Precio = 7500.00m },
                new Estudio { Id = 5, Nombre = "Estudio parasitológico", Precio = 1800.00m }
            );
           

            modelBuilder.Entity<MetodoPago>().HasData(
                new MetodoPago { Id = 1, Nombre = "Efectivo" },
                new MetodoPago { Id = 2, Nombre = "Pago Online / Mercado Pago" },
                new MetodoPago { Id = 3, Nombre = "Tarjeta" }
            );

            modelBuilder.Entity<Vacuna>().HasData(
                 new Vacuna { Id = 1, Nombre = "Antirrábica (Perros/Gatos)", Lote = "RAB-2024A", Precio = 3900.00m },
                 new Vacuna { Id = 2, Nombre = "Quíntuple Canina (Moquillo/Parvo)", Lote = "DHPPI-101", Precio = 11250.00m },
                 new Vacuna { Id = 3, Nombre = "Triple Felina (FVRCP)", Lote = "FVRCP-202", Precio = 5100.00m },
                 new Vacuna { Id = 4, Nombre = "Leucemia Felina (FeLV)", Lote = "FELV-303", Precio = 6400.00m },
                 new Vacuna { Id = 5, Nombre = "Bordetella (Tos de las Perreras)", Lote = "KC-404", Precio = 85000.00m }
            );

            // Relaciones con Usuario
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne()
                .HasForeignKey<Cliente>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veterinario>()
                .HasOne(v => v.Usuario)
                .WithOne()
                .HasForeignKey<Veterinario>(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veterinaria>()
                .HasOne(v => v.Usuario)
                .WithOne()
                .HasForeignKey<Veterinaria>(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones Turno
            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Mascota)
                .WithMany()
                .HasForeignKey(t => t.MascotaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Cliente)
                .WithMany(c => c.Turnos)
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);


            // Relaciones Historia Clinica
            modelBuilder.Entity<HistoriaClinica>()
                .HasOne(h => h.Mascota)
                .WithOne(m => m.HistoriaClinica)
                .HasForeignKey<HistoriaClinica>(h => h.MascotaId);

            // Relacion Atencion Veterinaria 

            modelBuilder.Entity<AtencionVeterinaria>()
                .HasOne(a => a.HistoriaClinica)
                .WithMany(h => h.Atenciones)
                .HasForeignKey(a => a.HistoriaClinicaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AtencionVeterinaria>()
                .HasOne(a => a.Tratamiento)
                .WithOne()
                .HasForeignKey<AtencionVeterinaria>(a => a.TratamientoId)
                 .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AtencionVeterinaria>()
                .HasOne(a => a.Veterinario)
                .WithMany()
                .HasForeignKey(a => a.VeterinarioId)
                .OnDelete(DeleteBehavior.Restrict);

        
            modelBuilder.Entity<AtencionVeterinaria>()
                .HasMany(a => a.Vacunas)
                .WithMany() 
                .UsingEntity(j => j.ToTable("AtencionVacunas")); 

            modelBuilder.Entity<AtencionVeterinaria>()
                .HasMany(a => a.EstudiosComplementarios)
                .WithMany()
                .UsingEntity(j => j.ToTable("AtencionEstudios"));

            // Chip
            modelBuilder.Entity<Chip>()
                .HasOne(c => c.Mascota)
                .WithOne(m => m.Chip)
                .HasForeignKey<Chip>(c => c.MascotaId);

            // Mascota Cliente
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Propietario)
                .WithMany(c => c.Mascotas)
                .HasForeignKey(m => m.ClienteId);

            // Relaciones con Veterinaria 
            modelBuilder.Entity<Veterinaria>()
                .HasMany(v => v.Veterinarios)
                .WithOne(v => v.Veterinaria)
                .HasForeignKey(v => v.VeterinariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veterinaria>()
                .HasMany(v => v.Clientes)
                .WithOne(c => c.Veterinaria)
                .HasForeignKey(c => c.VeterinariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veterinaria>()
                .HasOne(v => v.ConfiguracionVeterinaria)
                .WithOne(c => c.Veterinaria)
                .HasForeignKey<ConfiguracionVeterinaria>(c => c.VeterinariaId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
