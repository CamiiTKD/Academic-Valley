using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistroNota_RemoveNotaFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotaFinal",
                table: "Materias");

            migrationBuilder.CreateTable(
                name: "RegistrosNotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValorNota = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosNotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistroNotas_Materia",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistroNotas_MateriaId",
                table: "RegistrosNotas",
                column: "MateriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosNotas");

            migrationBuilder.AddColumn<decimal>(
                name: "NotaFinal",
                table: "Materias",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: true);
        }
    }
}
