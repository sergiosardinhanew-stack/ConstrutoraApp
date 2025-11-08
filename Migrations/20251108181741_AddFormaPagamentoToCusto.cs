using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstrutoraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFormaPagamentoToCusto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataVencimentoBoleto",
                table: "Custos",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataVencimentoParcela",
                table: "Custos",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaPagamento",
                table: "Custos",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "NumeroParcelas",
                table: "Custos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorParcela",
                table: "Custos",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataVencimentoBoleto",
                table: "Custos");

            migrationBuilder.DropColumn(
                name: "DataVencimentoParcela",
                table: "Custos");

            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "Custos");

            migrationBuilder.DropColumn(
                name: "NumeroParcelas",
                table: "Custos");

            migrationBuilder.DropColumn(
                name: "ValorParcela",
                table: "Custos");
        }
    }
}
