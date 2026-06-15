using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Commands.ActualizarHorario;

public record ActualizarHorarioCommand(
    Guid MateriaId,
    Guid HorarioId,
    DayOfWeek DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string? Aula,
    bool EsVirtual
) : IRequest;
