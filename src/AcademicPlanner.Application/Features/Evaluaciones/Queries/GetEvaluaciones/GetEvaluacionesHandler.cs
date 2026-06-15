using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Evaluaciones.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Evaluaciones.Queries.GetEvaluaciones;

public sealed class GetEvaluacionesHandler(
    IMateriaRepository materiaRepository,
    IEvaluacionRepository evaluacionRepository) : IRequestHandler<GetEvaluacionesQuery, IReadOnlyList<EvaluacionDto>>
{
    public async Task<IReadOnlyList<EvaluacionDto>> Handle(GetEvaluacionesQuery request, CancellationToken cancellationToken)
    {
        _ = await materiaRepository.GetByIdAsync(request.MateriaId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la materia con Id {request.MateriaId}.");

        var evaluaciones = await evaluacionRepository.GetByMateriaIdAsync(request.MateriaId, cancellationToken);

        return evaluaciones
            .Select(e => new EvaluacionDto(e.Id, e.MateriaId, e.Tipo, e.Estado, e.Fecha, e.Nota, e.Descripcion))
            .ToList()
            .AsReadOnly();
    }
}
