namespace AcademicPlanner.Application.Features.Agenda.DTOs;

public record HorarioCursadaDto(
    Guid Id,
    Guid MateriaId,
    DayOfWeek DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string? Aula,
    bool EsVirtual
);
