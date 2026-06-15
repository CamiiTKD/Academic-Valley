namespace AcademicPlanner.Application.Features.Agenda.DTOs;

public record HorarioMateriaDto(
    Guid MateriaId,
    string MateriaNombre,
    string MateriaCodigo,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string? Aula,
    bool EsVirtual
);

public record DiaAgendaDto(
    DayOfWeek DiaSemana,
    IReadOnlyList<HorarioMateriaDto> Horarios
);
