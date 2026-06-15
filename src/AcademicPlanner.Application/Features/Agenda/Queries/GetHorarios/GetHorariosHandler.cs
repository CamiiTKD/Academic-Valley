using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Agenda.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Queries.GetHorarios;

public sealed class GetHorariosHandler(
    IMateriaRepository materiaRepository,
    IHorarioCursadaRepository horarioRepository)
    : IRequestHandler<GetHorariosQuery, IReadOnlyList<HorarioCursadaDto>>
{
    public async Task<IReadOnlyList<HorarioCursadaDto>> Handle(GetHorariosQuery request, CancellationToken cancellationToken)
    {
        var existe = await materiaRepository.GetByIdAsync(request.MateriaId, cancellationToken);
        if (existe is null)
            throw new KeyNotFoundException($"No se encontró la materia con Id {request.MateriaId}.");

        var horarios = await horarioRepository.GetByMateriaIdAsync(request.MateriaId, cancellationToken);

        return horarios
            .Select(h => new HorarioCursadaDto(h.Id, h.MateriaId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Aula, h.EsVirtual))
            .ToList()
            .AsReadOnly();
    }
}
