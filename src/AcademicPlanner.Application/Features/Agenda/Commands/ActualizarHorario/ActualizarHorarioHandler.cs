using AcademicPlanner.Application.Common.Interfaces;
using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Commands.ActualizarHorario;

public sealed class ActualizarHorarioHandler(
    IHorarioCursadaRepository horarioRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ActualizarHorarioCommand>
{
    public async Task Handle(ActualizarHorarioCommand request, CancellationToken cancellationToken)
    {
        var horario = await horarioRepository.GetByIdAsync(request.HorarioId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró el horario con Id {request.HorarioId}.");

        if (horario.MateriaId != request.MateriaId)
            throw new KeyNotFoundException(
                $"El horario {request.HorarioId} no pertenece a la materia {request.MateriaId}.");

        horario.Actualizar(
            request.DiaSemana,
            request.HoraInicio,
            request.HoraFin,
            request.Aula,
            request.EsVirtual);

        horarioRepository.Update(horario);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
