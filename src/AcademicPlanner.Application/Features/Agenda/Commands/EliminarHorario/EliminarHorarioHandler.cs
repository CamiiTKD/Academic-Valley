using AcademicPlanner.Application.Common.Interfaces;
using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Commands.EliminarHorario;

public sealed class EliminarHorarioHandler(
    IHorarioCursadaRepository horarioRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<EliminarHorarioCommand>
{
    public async Task Handle(EliminarHorarioCommand request, CancellationToken cancellationToken)
    {
        var horario = await horarioRepository.GetByIdAsync(request.HorarioId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró el horario con Id {request.HorarioId}.");

        if (horario.MateriaId != request.MateriaId)
            throw new KeyNotFoundException(
                $"El horario {request.HorarioId} no pertenece a la materia {request.MateriaId}.");

        horarioRepository.Delete(horario);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
