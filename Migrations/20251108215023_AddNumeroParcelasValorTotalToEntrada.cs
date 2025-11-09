using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstrutoraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNumeroParcelasValorTotalToEntrada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroParcelas",
                table: "Entradas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotal",
                table: "Entradas",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroParcelas",
                table: "Entradas");

            migrationBuilder.DropColumn(
                name: "ValorTotal",
                table: "Entradas");
        }
    }
}
