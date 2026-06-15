using AcademicPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicPlanner.Infrastructure.Persistence.Configurations;

internal sealed class HorarioCursadaConfiguration : IEntityTypeConfiguration<HorarioCursada>
{
    public void Configure(EntityTypeBuilder<HorarioCursada> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.DiaSemana)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(h => h.HoraInicio)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(h => h.HoraFin)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(h => h.Aula)
            .HasMaxLength(100);

        builder.Property(h => h.EsVirtual)
            .IsRequired();

        builder.HasOne(h => h.Materia)
               .WithMany(m => m.Horarios)
               .HasForeignKey(h => h.MateriaId)
               .HasConstraintName("FK_HorariosCursada_Materia")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.MateriaId)
            .HasDatabaseName("IX_HorariosCursada_MateriaId");

        builder.ToTable("HorariosCursada");
    }
}
