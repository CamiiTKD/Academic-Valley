using AcademicPlanner.Domain.Entities;

namespace AcademicPlanner.Application.Common.Interfaces;

public interface IMateriaRepository
{
    Task<Materia?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Materia?> GetByIdWithNotasAsync(Guid id, CancellationToken ct = default);
    Task<Materia?> GetByIdWithCorrelativasAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Materia>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Materia>> GetAllWithCorrelativasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Materia>> GetAllWithCorrelativasAndNotasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Materia>> GetAllWithRegistroNotasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Materia>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Materia>> GetCursandoWithHorariosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Materia>> GetCursandoWithHorariosAndEvaluacionesAsync(CancellationToken ct = default);
    Task<bool> ExistsAsCorrelativaAsync(Guid materiaId, CancellationToken ct = default);
    Task AddAsync(Materia materia, CancellationToken ct = default);
    void Update(Materia materia);
    void Delete(Materia materia);
}
