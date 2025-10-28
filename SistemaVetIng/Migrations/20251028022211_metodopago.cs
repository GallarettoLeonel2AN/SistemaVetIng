using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class metodopago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MetodosPago",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 3, "Tarjeta" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MetodosPago",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
