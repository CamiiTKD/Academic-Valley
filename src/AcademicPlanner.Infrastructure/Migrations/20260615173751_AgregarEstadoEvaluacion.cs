using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoEvaluacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Evaluaciones",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Evaluaciones");
        }
    }
}
