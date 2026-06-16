using AcademicPlanner.Domain.Enums;
using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Commands.ActualizarEstado;

public record ActualizarEstadoMateriaCommand(
    Guid MateriaId,
    EstadoMateria NuevoEstado
) : IRequest;
