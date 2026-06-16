using AcademicPlanner.Domain.Enums;

namespace AcademicPlanner.Application.Features.Notas.DTOs;

public record RegistroNotaDto(
    Guid Id,
    int ValorNota,
    DateOnly Fecha,
    TipoNota Tipo,
    bool EsAplazo
);
