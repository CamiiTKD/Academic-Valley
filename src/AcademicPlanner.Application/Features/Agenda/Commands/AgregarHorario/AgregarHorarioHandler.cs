using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Agenda.DTOs;
using AcademicPlanner.Domain.Entities;
using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Commands.AgregarHorario;

public sealed class AgregarHorarioHandler(
    IMateriaRepository materiaRepository,
    IHorarioCursadaRepository horarioRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AgregarHorarioCommand, HorarioCursadaDto>
{
    public async Task<HorarioCursadaDto> Handle(AgregarHorarioCommand request, CancellationToken cancellationToken)
    {
        var materia = await materiaRepository.GetByIdAsync(request.MateriaId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la materia con Id {request.MateriaId}.");

        var horario = HorarioCursada.Crear(
            materia.Id,
            request.DiaSemana,
            request.HoraInicio,
            request.HoraFin,
            request.Aula,
            request.EsVirtual);

        await horarioRepository.AddAsync(horario, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new HorarioCursadaDto(
            horario.Id,
            horario.MateriaId,
            horario.DiaSemana,
            horario.HoraInicio,
            horario.HoraFin,
            horario.Aula,
            horario.EsVirtual);
    }
}
