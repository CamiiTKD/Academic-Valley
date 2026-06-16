using AcademicPlanner.Application.Features.Materias.DTOs;
using AcademicPlanner.Domain.Enums;
using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Commands.CrearMateria;

public record CrearMateriaCommand(
    string Nombre,
    string Codigo,
    int Cuatrimestre,
    IReadOnlyList<Guid>? CorrelativasIds,
    EstadoMateria EstadoInicial = EstadoMateria.Pendiente
) : IRequest<MateriaDto>;
