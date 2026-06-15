using AcademicPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicPlanner.Infrastructure.Persistence.Configurations;

internal sealed class MateriaConfiguration : IEntityTypeConfiguration<Materia>
{
    public void Configure(EntityTypeBuilder<Materia> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Codigo)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(m => m.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_Materias_Codigo");

        builder.Property(m => m.Cuatrimestre)
            .IsRequired();

        builder.Property(m => m.NotaFinal)
            .HasPrecision(4, 2);

        // Persiste el enum como string para legibilidad en la BD
        builder.Property(m => m.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // ─── Self-Referencing Many-to-Many: Correlatividades ───────────────────
        // Una Materia puede tener N materias correlativas requeridas.
        // Tabla intermedia: MateriasCorrelativas (MateriaId, CorrelativaRequeridaId)
        builder.HasMany(m => m.Correlativas)
               .WithMany()
               .UsingEntity<Dictionary<string, object>>(
                   "MateriasCorrelativas",
                   correlativa => correlativa
                       .HasOne<Materia>()
                       .WithMany()
                       .HasForeignKey("CorrelativaRequeridaId")
                       .HasConstraintName("FK_MateriasCorrelativas_CorrelativaRequerida")
                       .OnDelete(DeleteBehavior.Restrict),
                   materia => materia
                       .HasOne<Materia>()
                       .WithMany()
                       .HasForeignKey("MateriaId")
                       .HasConstraintName("FK_MateriasCorrelativas_Materia")
                       .OnDelete(DeleteBehavior.Cascade),
                   join =>
                   {
                       join.HasKey("MateriaId", "CorrelativaRequeridaId");
                       join.ToTable("MateriasCorrelativas");
                   });
    }
}
