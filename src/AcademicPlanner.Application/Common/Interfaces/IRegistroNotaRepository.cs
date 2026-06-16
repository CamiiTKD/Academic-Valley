using AcademicPlanner.Domain.Entities;

namespace AcademicPlanner.Application.Common.Interfaces;

public interface IRegistroNotaRepository
{
    Task AddAsync(RegistroNota registroNota, CancellationToken ct = default);
}
