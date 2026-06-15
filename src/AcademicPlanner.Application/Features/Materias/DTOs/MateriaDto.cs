using AcademicPlanner.Domain.Enums;

namespace AcademicPlanner.Application.Features.Materias.DTOs;

public record MateriaDto(
    Guid Id,
    string Nombre,
    string Codigo,
    int Cuatrimestre,
    decimal? NotaFinal,
    EstadoMateria Estado,
    IReadOnlyList<CorrelativaResumenDto> Correlativas
);

// Versión sin navegaciones anidadas para evitar ciclos en serialización JSON
public record CorrelativaResumenDto(Guid Id, string Nombre, string Codigo, EstadoMateria Estado);
