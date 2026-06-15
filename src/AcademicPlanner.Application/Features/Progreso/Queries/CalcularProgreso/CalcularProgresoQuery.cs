using AcademicPlanner.Application.Features.Progreso.DTOs;
using MediatR;

namespace AcademicPlanner.Application.Features.Progreso.Queries.CalcularProgreso;

public sealed record CalcularProgresoQuery : IRequest<ProgresoCarreraDto>;
