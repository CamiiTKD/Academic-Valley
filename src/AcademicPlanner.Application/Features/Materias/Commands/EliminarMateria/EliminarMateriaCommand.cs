using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Commands.EliminarMateria;

public record EliminarMateriaCommand(Guid MateriaId) : IRequest;
