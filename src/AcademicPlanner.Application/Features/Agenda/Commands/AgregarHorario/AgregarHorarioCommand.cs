using AcademicPlanner.Application.Features.Agenda.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Commands.AgregarHorario;

public record AgregarHorarioCommand(
    Guid MateriaId,
    DayOfWeek DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string? Aula,
    bool EsVirtual
) : IRequest<HorarioCursadaDto>;
