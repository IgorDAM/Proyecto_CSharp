using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinaApi.Migrations
{
    /// <inheritdoc />
    public partial class AmarreSetNullAlBorrarBarco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Amarres_Barcos_BarcoId",
                table: "Amarres");

            migrationBuilder.AddForeignKey(
                name: "FK_Amarres_Barcos_BarcoId",
                table: "Amarres",
                column: "BarcoId",
                principalTable: "Barcos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Amarres_Barcos_BarcoId",
                table: "Amarres");

            migrationBuilder.AddForeignKey(
                name: "FK_Amarres_Barcos_BarcoId",
                table: "Amarres",
                column: "BarcoId",
                principalTable: "Barcos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
