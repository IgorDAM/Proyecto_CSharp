using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaApi.Migrations
{
    /// <inheritdoc />
    public partial class AnadirOrganizador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrganizadorId",
                table: "Regatas",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizadores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tripulantes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BarcoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tripulantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tripulantes_Barcos_BarcoId",
                        column: x => x.BarcoId,
                        principalTable: "Barcos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Regatas_OrganizadorId",
                table: "Regatas",
                column: "OrganizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tripulantes_BarcoId",
                table: "Tripulantes",
                column: "BarcoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Regatas_Organizadores_OrganizadorId",
                table: "Regatas",
                column: "OrganizadorId",
                principalTable: "Organizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Regatas_Organizadores_OrganizadorId",
                table: "Regatas");

            migrationBuilder.DropTable(
                name: "Organizadores");

            migrationBuilder.DropTable(
                name: "Tripulantes");

            migrationBuilder.DropIndex(
                name: "IX_Regatas_OrganizadorId",
                table: "Regatas");

            migrationBuilder.DropColumn(
                name: "OrganizadorId",
                table: "Regatas");
        }
    }
}
