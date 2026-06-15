using AcademicPlanner.Domain.Enums;

namespace AcademicPlanner.Application.Features.Evaluaciones.DTOs;

public record EvaluacionDto(
    Guid Id,
    Guid MateriaId,
    TipoEvaluacion Tipo,
    EstadoEvaluacion Estado,
    DateOnly Fecha,
    decimal? Nota,
    string? Descripcion
);
