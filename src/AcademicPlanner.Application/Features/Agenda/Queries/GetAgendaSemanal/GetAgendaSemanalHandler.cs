using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Agenda.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Queries.GetAgendaSemanal;

public sealed class GetAgendaSemanalHandler(IMateriaRepository materiaRepository)
    : IRequestHandler<GetAgendaSemanalQuery, IReadOnlyList<DiaAgendaDto>>
{
    public async Task<IReadOnlyList<DiaAgendaDto>> Handle(
        GetAgendaSemanalQuery request,
        CancellationToken cancellationToken)
    {
        var materias = await materiaRepository.GetCursandoWithHorariosAsync(cancellationToken);

        return materias
            .SelectMany(m => m.Horarios.Select(h => (Materia: m, Horario: h)))
            .GroupBy(x => x.Horario.DiaSemana)
            .OrderBy(g => NormalizarDia(g.Key))
            .Select(g => new DiaAgendaDto(
                g.Key,
                g.OrderBy(x => x.Horario.HoraInicio)
                 .Select(x => new HorarioMateriaDto(
                     x.Materia.Id,
                     x.Materia.Nombre,
                     x.Materia.Codigo,
                     x.Horario.HoraInicio,
                     x.Horario.HoraFin,
                     x.Horario.Aula,
                     x.Horario.EsVirtual))
                 .ToList()
                 .AsReadOnly()))
            .ToList()
            .AsReadOnly();
    }

    // Lunes=1 … Sábado=6, Domingo=7 (en .NET Sunday=0, lo normalizamos al final)
    private static int NormalizarDia(DayOfWeek dia) => dia == DayOfWeek.Sunday ? 7 : (int)dia;
}
