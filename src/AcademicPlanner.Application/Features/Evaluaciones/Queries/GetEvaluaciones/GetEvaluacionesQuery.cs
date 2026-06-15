using AcademicPlanner.Application.Features.Evaluaciones.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Evaluaciones.Queries.GetEvaluaciones;

public record GetEvaluacionesQuery(Guid MateriaId) : IRequest<IReadOnlyList<EvaluacionDto>>;
