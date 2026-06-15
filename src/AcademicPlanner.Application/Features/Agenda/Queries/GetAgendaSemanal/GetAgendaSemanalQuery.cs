using AcademicPlanner.Application.Features.Agenda.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Agenda.Queries.GetAgendaSemanal;

public record GetAgendaSemanalQuery() : IRequest<IReadOnlyList<DiaAgendaDto>>;
