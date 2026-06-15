using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Commands.EliminarHorario;

public record EliminarHorarioCommand(Guid MateriaId, Guid HorarioId) : IRequest;
