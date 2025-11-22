using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class Memento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AtencionMementos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtencionVeterinariaId = table.Column<int>(type: "int", nullable: false),
                    FechaVersion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioEditor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotivoCambio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnostico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PesoMascota = table.Column<float>(type: "real", nullable: false),
                    TratamientoMedicamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TratamientoDosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TratamientoFrecuencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TratamientoDuracion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TratamientoObservaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtencionMementos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtencionMementos");
        }
    }
}
