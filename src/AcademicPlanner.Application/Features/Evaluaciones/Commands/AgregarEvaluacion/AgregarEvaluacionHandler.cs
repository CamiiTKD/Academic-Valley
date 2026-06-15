using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Evaluaciones.DTOs;
using AcademicPlanner.Domain.Entities;
using MediatR;

namespace AcademicPlanner.Application.Features.Evaluaciones.Commands.AgregarEvaluacion;

public sealed class AgregarEvaluacionHandler(
    IMateriaRepository materiaRepository,
    IEvaluacionRepository evaluacionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AgregarEvaluacionCommand, EvaluacionDto>
{
    public async Task<EvaluacionDto> Handle(AgregarEvaluacionCommand request, CancellationToken cancellationToken)
    {
        var materia = await materiaRepository.GetByIdAsync(request.MateriaId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la materia con Id {request.MateriaId}.");

        var evaluacion = Evaluacion.Crear(materia.Id, request.Tipo, request.Fecha, request.Descripcion);

        await evaluacionRepository.AddAsync(evaluacion, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EvaluacionDto(
            evaluacion.Id,
            evaluacion.MateriaId,
            evaluacion.Tipo,
            evaluacion.Estado,
            evaluacion.Fecha,
            evaluacion.Nota,
            evaluacion.Descripcion);
    }
}
