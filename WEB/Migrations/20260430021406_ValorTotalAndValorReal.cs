using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB.Migrations
{
    /// <inheritdoc />
    public partial class ValorTotalAndValorReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValorReal",
                table: "IndicadorDeArea",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotal",
                table: "IndicadorDeArea",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValorReal",
                table: "Indicador",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValorTotal",
                table: "Indicador",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorReal",
                table: "IndicadorDeArea");

            migrationBuilder.DropColumn(
                name: "ValorTotal",
                table: "IndicadorDeArea");

            migrationBuilder.DropColumn(
                name: "ValorReal",
                table: "Indicador");

            migrationBuilder.DropColumn(
                name: "ValorTotal",
                table: "Indicador");
        }
    }
}
