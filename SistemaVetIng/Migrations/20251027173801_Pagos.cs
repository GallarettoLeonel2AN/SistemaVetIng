using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaVetIng.Migrations
{
    /// <inheritdoc />
    public partial class Pagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_AtencionesVeterinarias_AtencionVeterinariaId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_AtencionVeterinariaId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "AtencionVeterinariaId",
                table: "Pagos");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "MetodosPago",
                newName: "Nombre");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Pagos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MontoTotal",
                table: "Pagos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PagoId",
                table: "AtencionesVeterinarias",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "MetodosPago",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Efectivo" },
                    { 2, "Tarjeta" },
                    { 3, "Mercado Pago" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtencionesVeterinarias_PagoId",
                table: "AtencionesVeterinarias",
                column: "PagoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AtencionesVeterinarias_Pagos_PagoId",
                table: "AtencionesVeterinarias",
                column: "PagoId",
                principalTable: "Pagos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AtencionesVeterinarias_Pagos_PagoId",
                table: "AtencionesVeterinarias");

            migrationBuilder.DropIndex(
                name: "IX_AtencionesVeterinarias_PagoId",
                table: "AtencionesVeterinarias");

            migrationBuilder.DeleteData(
                table: "MetodosPago",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MetodosPago",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MetodosPago",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MontoTotal",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "PagoId",
                table: "AtencionesVeterinarias");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "MetodosPago",
                newName: "Tipo");

            migrationBuilder.AddColumn<int>(
                name: "AtencionVeterinariaId",
                table: "Pagos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_AtencionVeterinariaId",
                table: "Pagos",
                column: "AtencionVeterinariaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_AtencionesVeterinarias_AtencionVeterinariaId",
                table: "Pagos",
                column: "AtencionVeterinariaId",
                principalTable: "AtencionesVeterinarias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
