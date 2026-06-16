using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Infrastructure.Persistence;

namespace AcademicPlanner.Infrastructure.Repositories;

internal sealed class RegistroNotaRepository(AcademicPlannerDbContext context) : IRegistroNotaRepository
{
    public async Task AddAsync(RegistroNota registroNota, CancellationToken ct = default)
        => await context.RegistrosNotas.AddAsync(registroNota, ct);
}
