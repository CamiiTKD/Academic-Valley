namespace AcademicPlanner.Application.Features.Progreso.DTOs;

public record ProgresoCarreraDto(
    int TotalMaterias,
    int MateriasAprobadas,
    decimal PorcentajeProgreso,
    // Promedio contando TODAS las notas registradas (incluyendo aplazos/desaprobados)
    decimal PromedioConAplazos,
    // Promedio contando solo materias con estado Aprobada
    decimal PromedioSinAplazos
)
{
    public static ProgresoCarreraDto Vacio() =>
        new(0, 0, 0m, 0m, 0m);
};
