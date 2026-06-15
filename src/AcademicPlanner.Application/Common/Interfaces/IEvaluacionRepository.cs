using AcademicPlanner.Domain.Entities;

namespace AcademicPlanner.Application.Common.Interfaces;

public interface IEvaluacionRepository
{
    Task<Evaluacion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Evaluacion>> GetByMateriaIdAsync(Guid materiaId, CancellationToken ct = default);
    Task AddAsync(Evaluacion evaluacion, CancellationToken ct = default);
    void Delete(Evaluacion evaluacion);
}
