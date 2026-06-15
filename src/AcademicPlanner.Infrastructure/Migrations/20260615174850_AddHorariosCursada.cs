using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHorariosCursada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HorariosCursada",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaSemana = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Aula = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EsVirtual = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosCursada", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosCursada_Materia",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorariosCursada_MateriaId",
                table: "HorariosCursada",
                column: "MateriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorariosCursada");
        }
    }
}
