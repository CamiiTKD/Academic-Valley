using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Infrastructure.Repositories;

internal sealed class HorarioCursadaRepository(AcademicPlannerDbContext context) : IHorarioCursadaRepository
{
    public async Task<HorarioCursada?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.HorariosCursada.FirstOrDefaultAsync(h => h.Id == id, ct);

    public async Task AddAsync(HorarioCursada horario, CancellationToken ct = default)
        => await context.HorariosCursada.AddAsync(horario, ct);

    public void Update(HorarioCursada horario)
        => context.HorariosCursada.Update(horario);

    public void Delete(HorarioCursada horario)
        => context.HorariosCursada.Remove(horario);
}
