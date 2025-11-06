using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerrosPeligrososApi.Migrations
{
    /// <inheritdoc />
    public partial class primera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerrosPeligrosos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Raza = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MascotaIdOriginal = table.Column<int>(type: "int", nullable: false),
                    ClienteDni = table.Column<long>(type: "bigint", nullable: false),
                    ClienteNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClienteApellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaRegistroApi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerrosPeligrosos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChipsPerroPeligroso",
                columns: table => new
                {
                    PerroPeligrosoId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChipsPerroPeligroso", x => x.PerroPeligrosoId);
                    table.ForeignKey(
                        name: "FK_ChipsPerroPeligroso_PerrosPeligrosos_PerroPeligrosoId",
                        column: x => x.PerroPeligrosoId,
                        principalTable: "PerrosPeligrosos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChipsPerroPeligroso");

            migrationBuilder.DropTable(
                name: "PerrosPeligrosos");
        }
    }
}
