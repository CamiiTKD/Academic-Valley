using AcademicPlanner.Application.Features.Notas.DTOs;
using AcademicPlanner.Domain.Enums;
using MediatR;

namespace AcademicPlanner.Application.Features.Notas.Commands.AgregarNota;

public record AgregarNotaCommand(
    Guid MateriaId,
    int ValorNota,
    DateOnly Fecha,
    TipoNota Tipo
) : IRequest<RegistroNotaDto>;
