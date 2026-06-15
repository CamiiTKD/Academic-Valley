using AcademicPlanner.Application.Features.Agenda.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Queries.GetHorarios;

public record GetHorariosQuery(Guid MateriaId) : IRequest<IReadOnlyList<HorarioCursadaDto>>;
