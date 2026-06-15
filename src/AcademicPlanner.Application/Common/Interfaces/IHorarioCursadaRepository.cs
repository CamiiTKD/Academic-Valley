using AcademicPlanner.Domain.Entities;

namespace AcademicPlanner.Application.Common.Interfaces;

public interface IHorarioCursadaRepository
{
    Task<HorarioCursada?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioCursada>> GetByMateriaIdAsync(Guid materiaId, CancellationToken ct = default);
    Task AddAsync(HorarioCursada horario, CancellationToken ct = default);
    void Update(HorarioCursada horario);
    void Delete(HorarioCursada horario);
}
