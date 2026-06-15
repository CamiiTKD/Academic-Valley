using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using AcademicPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Infrastructure.Repositories;

internal sealed class MateriaRepository(AcademicPlannerDbContext context) : IMateriaRepository
{
    public async Task<Materia?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Materias
            .Include(m => m.Evaluaciones)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<Materia?> GetByIdWithCorrelativasAsync(Guid id, CancellationToken ct = default)
        => await context.Materias
            .Include(m => m.Correlativas)
            .Include(m => m.Evaluaciones)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Materia>> GetCursandoWithHorariosAsync(CancellationToken ct = default)
        => await context.Materias
            .AsNoTracking()
            .Where(m => m.Estado == EstadoMateria.Cursando)
            .Include(m => m.Horarios)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Materia>> GetCursandoWithHorariosAndEvaluacionesAsync(CancellationToken ct = default)
        => await context.Materias
            .AsNoTracking()
            .Where(m => m.Estado == EstadoMateria.Cursando)
            .Include(m => m.Horarios)
            .Include(m => m.Evaluaciones)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Materia>> GetAllAsync(CancellationToken ct = default)
        => await context.Materias
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Materia>> GetAllWithCorrelativasAsync(CancellationToken ct = default)
        => await context.Materias
            .AsNoTracking()
            .Include(m => m.Correlativas)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Materia>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        => await context.Materias
            .Where(m => ids.Contains(m.Id))
            .ToListAsync(ct);

    public async Task<bool> ExistsAsCorrelativaAsync(Guid materiaId, CancellationToken ct = default)
        => await context.Materias
            .AnyAsync(m => m.Correlativas.Any(c => c.Id == materiaId), ct);

    public async Task AddAsync(Materia materia, CancellationToken ct = default)
        => await context.Materias.AddAsync(materia, ct);

    public void Update(Materia materia)
        => context.Materias.Update(materia);

    public void Delete(Materia materia)
        => context.Materias.Remove(materia);
}
