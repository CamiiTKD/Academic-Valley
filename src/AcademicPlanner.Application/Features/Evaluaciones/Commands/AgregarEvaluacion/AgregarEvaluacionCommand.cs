using AcademicPlanner.Application.Features.Evaluaciones.DTOs;
using AcademicPlanner.Domain.Enums;
using MediatR;

namespace AcademicPlanner.Application.Features.Evaluaciones.Commands.AgregarEvaluacion;

public record AgregarEvaluacionCommand(
    Guid MateriaId,
    TipoEvaluacion Tipo,
    DateOnly Fecha,
    string? Descripcion
) : IRequest<EvaluacionDto>;
