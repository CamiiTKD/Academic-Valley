using AcademicPlanner.Domain.Common;
using AcademicPlanner.Domain.Enums;

namespace AcademicPlanner.Domain.Entities;

public class RegistroNota : BaseEntity
{
    public Guid MateriaId { get; private set; }
    public int ValorNota { get; private set; }
    public DateOnly Fecha { get; private set; }
    public TipoNota Tipo { get; private set; }

    protected RegistroNota() { }

    public static RegistroNota Crear(Guid materiaId, int valorNota, DateOnly fecha, TipoNota tipo)
    {
        if (materiaId == Guid.Empty)
            throw new ArgumentException("El Id de la materia no puede estar vacío.", nameof(materiaId));

        if (valorNota < 1 || valorNota > 10)
            throw new ArgumentOutOfRangeException(nameof(valorNota), "La nota debe estar entre 1 y 10.");

        return new RegistroNota
        {
            MateriaId = materiaId,
            ValorNota = valorNota,
            Fecha = fecha,
            Tipo = tipo,
        };
    }
}
