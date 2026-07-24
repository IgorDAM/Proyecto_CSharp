using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Barcos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Eslora = table.Column<int>(type: "int", nullable: false),
                    Manga = table.Column<int>(type: "int", nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barcos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regatas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Lugar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Distancia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Amarres",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ubicacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Precio = table.Column<double>(type: "float", nullable: false),
                    Profundidad = table.Column<int>(type: "int", nullable: false),
                    Longitud = table.Column<int>(type: "int", nullable: false),
                    Electricidad = table.Column<bool>(type: "bit", nullable: false),
                    BarcoId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amarres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Amarres_Barcos_BarcoId",
                        column: x => x.BarcoId,
                        principalTable: "Barcos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BarcoRegata",
                columns: table => new
                {
                    BarcosId = table.Column<long>(type: "bigint", nullable: false),
                    RegatasId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcoRegata", x => new { x.BarcosId, x.RegatasId });
                    table.ForeignKey(
                        name: "FK_BarcoRegata_Barcos_BarcosId",
                        column: x => x.BarcosId,
                        principalTable: "Barcos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BarcoRegata_Regatas_RegatasId",
                        column: x => x.RegatasId,
                        principalTable: "Regatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amarres_BarcoId",
                table: "Amarres",
                column: "BarcoId",
                unique: true,
                filter: "[BarcoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BarcoRegata_RegatasId",
                table: "BarcoRegata",
                column: "RegatasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amarres");

            migrationBuilder.DropTable(
                name: "BarcoRegata");

            migrationBuilder.DropTable(
                name: "Barcos");

            migrationBuilder.DropTable(
                name: "Regatas");
        }
    }
}
