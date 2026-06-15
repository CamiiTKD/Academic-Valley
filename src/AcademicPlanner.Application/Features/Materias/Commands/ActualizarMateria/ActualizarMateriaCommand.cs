using AcademicPlanner.Application.Features.Materias.DTOs;
using AcademicPlanner.Domain.Enums;
using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Commands.ActualizarMateria;

public record ActualizarMateriaCommand(
    Guid Id,
    string Nombre,
    string Codigo,
    int Cuatrimestre,
    EstadoMateria Estado,
    decimal? NotaFinal
) : IRequest<MateriaDto>;
