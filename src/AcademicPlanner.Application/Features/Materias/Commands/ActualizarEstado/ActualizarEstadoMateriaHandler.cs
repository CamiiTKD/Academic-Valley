using AcademicPlanner.Application.Common.Interfaces;
using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Commands.ActualizarEstado;

public sealed class ActualizarEstadoMateriaHandler(
    IMateriaRepository materiaRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ActualizarEstadoMateriaCommand>
{
    public async Task Handle(ActualizarEstadoMateriaCommand request, CancellationToken cancellationToken)
    {
        var materia = await materiaRepository.GetByIdAsync(request.MateriaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Materia con Id '{request.MateriaId}' no encontrada.");

        materia.ActualizarEstado(request.NuevoEstado);
        materiaRepository.Update(materia);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
