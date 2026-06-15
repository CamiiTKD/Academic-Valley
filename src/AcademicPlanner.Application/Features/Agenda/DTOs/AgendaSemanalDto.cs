using AcademicPlanner.Domain.Enums;

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

public record AlertaEvaluacionDto(
    Guid MateriaId,
    string MateriaNombre,
    TipoEvaluacion Tipo,
    DateOnly Fecha,
    string? Descripcion
);

public record DiaAgendaDto(
    DayOfWeek DiaSemana,
    IReadOnlyList<HorarioMateriaDto> Horarios,
    IReadOnlyList<AlertaEvaluacionDto> Alertas
);
