using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Materias.DTOs;
using AcademicPlanner.Application.Features.Notas.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Queries.GetMaterias;

public sealed class GetMateriasHandler(IMateriaRepository materiaRepository)
    : IRequestHandler<GetMateriasQuery, IReadOnlyList<MateriaDto>>
{
    public async Task<IReadOnlyList<MateriaDto>> Handle(GetMateriasQuery request, CancellationToken cancellationToken)
    {
        var materias = await materiaRepository.GetAllWithCorrelativasAndNotasAsync(cancellationToken);

        var query = materias.AsEnumerable();

        if (request.Estado.HasValue)
            query = query.Where(m => m.Estado == request.Estado.Value);

        if (request.Cuatrimestre.HasValue)
            query = query.Where(m => m.Cuatrimestre == request.Cuatrimestre.Value);

        return query
            .Select(m => new MateriaDto(
                m.Id,
                m.Nombre,
                m.Codigo,
                m.Cuatrimestre,
                m.Estado,
                m.Correlativas
                    .Select(c => new CorrelativaResumenDto(c.Id, c.Nombre, c.Codigo, c.Estado))
                    .ToList()
                    .AsReadOnly(),
                m.RegistroNotas
                    .OrderBy(n => n.Fecha)
                    .Select(n => new RegistroNotaDto(n.Id, n.ValorNota, n.Fecha, n.Tipo, n.ValorNota < 4))
                    .ToList()
                    .AsReadOnly()
            ))
            .ToList()
            .AsReadOnly();
    }
}
