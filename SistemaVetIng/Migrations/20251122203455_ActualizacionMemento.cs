using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionMemento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstudiosSnapshot",
                table: "AtencionMementos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VacunasSnapshot",
                table: "AtencionMementos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstudiosSnapshot",
                table: "AtencionMementos");

            migrationBuilder.DropColumn(
                name: "VacunasSnapshot",
                table: "AtencionMementos");
        }
    }
}
