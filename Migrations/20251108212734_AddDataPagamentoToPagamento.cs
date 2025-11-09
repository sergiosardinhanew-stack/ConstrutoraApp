using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstrutoraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDataPagamentoToPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataPagamento",
                table: "Pagamentos",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataPagamento",
                table: "Pagamentos");
        }
    }
}
