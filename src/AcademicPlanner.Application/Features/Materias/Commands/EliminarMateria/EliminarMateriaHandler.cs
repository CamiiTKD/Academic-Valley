using AcademicPlanner.Application.Common.Interfaces;
using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Commands.EliminarMateria;

public sealed class EliminarMateriaHandler(
    IMateriaRepository materiaRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<EliminarMateriaCommand>
{
    public async Task Handle(EliminarMateriaCommand request, CancellationToken cancellationToken)
    {
        var materia = await materiaRepository.GetByIdAsync(request.MateriaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Materia con Id '{request.MateriaId}' no encontrada.");

        var esCorrelativaActiva = await materiaRepository.ExistsAsCorrelativaAsync(request.MateriaId, cancellationToken);
        if (esCorrelativaActiva)
            throw new InvalidOperationException(
                "No se puede eliminar la materia porque es correlativa requerida de otras materias del plan de estudios.");

        materiaRepository.Delete(materia);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
