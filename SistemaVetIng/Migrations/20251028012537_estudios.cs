using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class estudios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MetodosPago",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "Informe",
                table: "Estudios");

            migrationBuilder.UpdateData(
                table: "MetodosPago",
                keyColumn: "Id",
                keyValue: 2,
                column: "Nombre",
                value: "Pago Online / Mercado Pago");

            migrationBuilder.InsertData(
                table: "Vacunas",
                columns: new[] { "Id", "AtencionVeterinariaId", "Lote", "Nombre", "Precio" },
                values: new object[,]
                {
                    { 1, null, "RAB-2024A", "Antirrábica (Perros/Gatos)", 3900.00m },
                    { 2, null, "DHPPI-101", "Quíntuple Canina (Moquillo/Parvo)", 11250.00m },
                    { 3, null, "FVRCP-202", "Triple Felina (FVRCP)", 5100.00m },
                    { 4, null, "FELV-303", "Leucemia Felina (FeLV)", 6400.00m },
                    { 5, null, "KC-404", "Bordetella (Tos de las Perreras)", 85000.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DeleteData(
                table: "Vacunas",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "Informe",
                table: "Estudios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Estudios",
                keyColumn: "Id",
                keyValue: 1,
                column: "Informe",
                value: null);

            migrationBuilder.UpdateData(
                table: "Estudios",
                keyColumn: "Id",
                keyValue: 2,
                column: "Informe",
                value: null);

            migrationBuilder.UpdateData(
                table: "Estudios",
                keyColumn: "Id",
                keyValue: 3,
                column: "Informe",
                value: null);

            migrationBuilder.UpdateData(
                table: "Estudios",
                keyColumn: "Id",
                keyValue: 4,
                column: "Informe",
                value: null);

            migrationBuilder.UpdateData(
                table: "Estudios",
                keyColumn: "Id",
                keyValue: 5,
                column: "Informe",
                value: null);

            migrationBuilder.UpdateData(
                table: "MetodosPago",
                keyColumn: "Id",
                keyValue: 2,
                column: "Nombre",
                value: "Tarjeta");

            migrationBuilder.InsertData(
                table: "MetodosPago",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 3, "Mercado Pago" });
        }
    }
}
