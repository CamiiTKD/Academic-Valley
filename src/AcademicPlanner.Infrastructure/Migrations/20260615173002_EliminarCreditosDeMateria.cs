using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EliminarCreditosDeMateria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Creditos",
                table: "Materias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Creditos",
                table: "Materias",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
