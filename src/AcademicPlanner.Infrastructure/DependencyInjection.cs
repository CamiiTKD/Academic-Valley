using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Infrastructure.Persistence;
using AcademicPlanner.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("Connection string 'PostgreSQL' no encontrada.");

        services.AddDbContext<AcademicPlannerDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AcademicPlannerDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        // IUnitOfWork lo implementa el DbContext directamente
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AcademicPlannerDbContext>());
        services.AddScoped<IMateriaRepository, MateriaRepository>();
        services.AddScoped<IEvaluacionRepository, EvaluacionRepository>();
        services.AddScoped<IHorarioCursadaRepository, HorarioCursadaRepository>();
        services.AddScoped<IRegistroNotaRepository, RegistroNotaRepository>();

        return services;
    }
}
