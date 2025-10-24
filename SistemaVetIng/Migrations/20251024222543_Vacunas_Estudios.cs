using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class Vacunas_Estudios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Vacunas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Vacunas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Vacunas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Vacunas",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "FechaAplicacion",
                table: "Vacunas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAplicacion",
                table: "Vacunas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "Vacunas",
                columns: new[] { "Id", "AtencionVeterinariaId", "FechaAplicacion", "Lote", "Nombre", "Precio" },
                values: new object[,]
                {
                    { 1, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lote-A123", "Vacuna Antirrábica", 2500.00m },
                    { 2, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lote-B456", "Vacuna Quíntuple Canina", 3200.00m },
                    { 3, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lote-C789", "Vacuna Triple Felina", 2800.00m },
                    { 4, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lote-D012", "Vacuna de la Tos de las Perreras", 2000.00m }
                });
        }
    }
}
