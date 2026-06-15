using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Infrastructure.Repositories;

internal sealed class EvaluacionRepository(AcademicPlannerDbContext context) : IEvaluacionRepository
{
    public async Task<Evaluacion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Evaluaciones.FindAsync([id], ct);

    public async Task<IReadOnlyList<Evaluacion>> GetByMateriaIdAsync(Guid materiaId, CancellationToken ct = default)
        => await context.Evaluaciones
            .AsNoTracking()
            .Where(e => e.MateriaId == materiaId)
            .OrderBy(e => e.Fecha)
            .ToListAsync(ct);

    public async Task AddAsync(Evaluacion evaluacion, CancellationToken ct = default)
        => await context.Evaluaciones.AddAsync(evaluacion, ct);

    public void Delete(Evaluacion evaluacion)
        => context.Evaluaciones.Remove(evaluacion);
}
