using AcademicPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicPlanner.Infrastructure.Persistence.Configurations;

internal sealed class RegistroNotaConfiguration : IEntityTypeConfiguration<RegistroNota>
{
    public void Configure(EntityTypeBuilder<RegistroNota> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ValorNota)
            .IsRequired();

        builder.Property(r => r.Fecha)
            .IsRequired();

        builder.Property(r => r.Tipo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne<Materia>()
               .WithMany(m => m.RegistroNotas)
               .HasForeignKey(r => r.MateriaId)
               .HasConstraintName("FK_RegistroNotas_Materia")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.MateriaId)
            .HasDatabaseName("IX_RegistroNotas_MateriaId");
    }
}
