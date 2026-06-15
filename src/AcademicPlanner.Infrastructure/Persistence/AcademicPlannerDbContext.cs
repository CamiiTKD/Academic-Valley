using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Infrastructure.Persistence;

public class AcademicPlannerDbContext(DbContextOptions<AcademicPlannerDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Materia> Materias => Set<Materia>();
    public DbSet<Evaluacion> Evaluaciones => Set<Evaluacion>();
    public DbSet<HorarioCursada> HorariosCursada => Set<HorarioCursada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas las IEntityTypeConfiguration<T> del assembly automáticamente
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AcademicPlannerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);
}
