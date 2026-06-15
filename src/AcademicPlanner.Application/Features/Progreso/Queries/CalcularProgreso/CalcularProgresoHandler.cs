using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Progreso.DTOs;
using AcademicPlanner.Domain.Enums;
using MediatR;

namespace AcademicPlanner.Application.Features.Progreso.Queries.CalcularProgreso;

public sealed class CalcularProgresoHandler(IMateriaRepository materiaRepository)
    : IRequestHandler<CalcularProgresoQuery, ProgresoCarreraDto>
{
    public async Task<ProgresoCarreraDto> Handle(CalcularProgresoQuery request, CancellationToken cancellationToken)
    {
        var materias = await materiaRepository.GetAllAsync(cancellationToken);

        if (materias.Count == 0)
            return ProgresoCarreraDto.Vacio();

        var aprobadas = materias.Where(m => m.Estado == EstadoMateria.Aprobada).ToList();

        var porcentajeProgreso = Math.Round((decimal)aprobadas.Count / materias.Count * 100, 2);

        // Con aplazos: promedio de TODAS las notas finales registradas (incluye reprobados)
        var todasLasNotas = materias
            .Where(m => m.NotaFinal.HasValue)
            .Select(m => m.NotaFinal!.Value)
            .ToList();

        // Sin aplazos: promedio solo de materias aprobadas
        var notasAprobadas = aprobadas
            .Where(m => m.NotaFinal.HasValue)
            .Select(m => m.NotaFinal!.Value)
            .ToList();

        var promedioConAplazos = todasLasNotas.Count > 0
            ? Math.Round(todasLasNotas.Average(), 2)
            : 0m;

        var promedioSinAplazos = notasAprobadas.Count > 0
            ? Math.Round(notasAprobadas.Average(), 2)
            : 0m;

        return new ProgresoCarreraDto(
            materias.Count,
            aprobadas.Count,
            porcentajeProgreso,
            promedioConAplazos,
            promedioSinAplazos
        );
    }
}
