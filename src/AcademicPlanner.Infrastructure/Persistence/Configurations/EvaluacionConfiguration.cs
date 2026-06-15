using AcademicPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicPlanner.Infrastructure.Persistence.Configurations;

internal sealed class EvaluacionConfiguration : IEntityTypeConfiguration<Evaluacion>
{
    public void Configure(EntityTypeBuilder<Evaluacion> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Tipo)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Fecha)
            .IsRequired();

        builder.Property(e => e.Nota)
            .HasPrecision(4, 2);

        builder.Property(e => e.Descripcion)
            .HasMaxLength(500);

        // ─── Relación Uno a Muchos: Materia → Evaluaciones ─────────────────────
        builder.HasOne(e => e.Materia)
               .WithMany(m => m.Evaluaciones)
               .HasForeignKey(e => e.MateriaId)
               .HasConstraintName("FK_Evaluaciones_Materia")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.MateriaId)
            .HasDatabaseName("IX_Evaluaciones_MateriaId");

        builder.HasIndex(e => new { e.MateriaId, e.Fecha })
            .HasDatabaseName("IX_Evaluaciones_MateriaId_Fecha");
    }
}
