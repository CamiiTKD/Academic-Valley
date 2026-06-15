using AcademicPlanner.Application.Features.Materias.DTOs;
using AcademicPlanner.Domain.Enums;
using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Queries.GetMaterias;

public record GetMateriasQuery(
    EstadoMateria? Estado,
    int? Cuatrimestre
) : IRequest<IReadOnlyList<MateriaDto>>;
