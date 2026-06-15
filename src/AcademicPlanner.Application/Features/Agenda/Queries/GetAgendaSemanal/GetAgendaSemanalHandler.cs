using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Agenda.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Queries.GetAgendaSemanal;

public sealed class GetAgendaSemanalHandler(IMateriaRepository materiaRepository)
    : IRequestHandler<GetAgendaSemanalQuery, IReadOnlyList<DiaAgendaDto>>
{
    // Lunes → Domingo, en orden de pantalla
    private static readonly DayOfWeek[] DiasOrdenados =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    public async Task<IReadOnlyList<DiaAgendaDto>> Handle(
        GetAgendaSemanalQuery request,
        CancellationToken cancellationToken)
    {
        var materias = await materiaRepository.GetCursandoWithHorariosAndEvaluacionesAsync(cancellationToken);

        var hoy    = DateOnly.FromDateTime(DateTime.Today);
        var limite = hoy.AddDays(7);

        // Horarios agrupados por día de la semana
        var horariosPorDia = materias
            .SelectMany(m => m.Horarios.Select(h => (Materia: m, Horario: h)))
            .GroupBy(x => x.Horario.DiaSemana)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<HorarioMateriaDto>)g
                    .OrderBy(x => x.Horario.HoraInicio)
                    .Select(x => new HorarioMateriaDto(
                        x.Materia.Id,
                        x.Materia.Nombre,
                        x.Materia.Codigo,
                        x.Horario.HoraInicio,
                        x.Horario.HoraFin,
                        x.Horario.Aula,
                        x.Horario.EsVirtual))
                    .ToList()
                    .AsReadOnly());

        // Alertas: evaluaciones pendientes dentro de los próximos 7 días, agrupadas por día de la semana de su fecha
        var alertasPorDia = materias
            .SelectMany(m => m.Evaluaciones
                .Where(e => e.Fecha >= hoy && e.Fecha <= limite)
                .Select(e => (Materia: m, Evaluacion: e)))
            .GroupBy(x => x.Evaluacion.Fecha.DayOfWeek)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<AlertaEvaluacionDto>)g
                    .OrderBy(x => x.Evaluacion.Fecha)
                    .Select(x => new AlertaEvaluacionDto(
                        x.Materia.Id,
                        x.Materia.Nombre,
                        x.Evaluacion.Tipo,
                        x.Evaluacion.Fecha,
                        x.Evaluacion.Descripcion))
                    .ToList()
                    .AsReadOnly());

        // Devuelve los 7 días en orden, siempre (incluso si están vacíos)
        return DiasOrdenados
            .Select(dia => new DiaAgendaDto(
                dia,
                horariosPorDia.TryGetValue(dia, out var h) ? h : [],
                alertasPorDia.TryGetValue(dia, out var a) ? a : []))
            .ToList()
            .AsReadOnly();
    }
}
