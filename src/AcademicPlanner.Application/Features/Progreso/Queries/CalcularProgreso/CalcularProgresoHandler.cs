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
        var materias = await materiaRepository.GetAllWithRegistroNotasAsync(cancellationToken);

        if (materias.Count == 0)
            return ProgresoCarreraDto.Vacio();

        var aprobadas = materias.Where(m => m.Estado == EstadoMateria.Aprobada).ToList();

        var porcentajeProgreso = Math.Round((decimal)aprobadas.Count / materias.Count * 100, 2);

        // Solo notas de tipo ExamenFinal participan en los promedios reglamentarios
        var notasExamenFinal = materias
            .SelectMany(m => m.RegistroNotas)
            .Where(n => n.Tipo == TipoNota.ExamenFinal)
            .Select(n => n.ValorNota)
            .ToList();

        // Con aplazos: todas las notas de ExamenFinal (incluyendo desaprobados < 4)
        var promedioConAplazos = notasExamenFinal.Count > 0
            ? Math.Round((decimal)notasExamenFinal.Average(), 2)
            : 0m;

        // Sin aplazos: solo notas de ExamenFinal >= 4 (aprobadas)
        var notasAprobadas = notasExamenFinal.Where(n => n >= 4).ToList();

        var promedioSinAplazos = notasAprobadas.Count > 0
            ? Math.Round((decimal)notasAprobadas.Average(), 2)
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
