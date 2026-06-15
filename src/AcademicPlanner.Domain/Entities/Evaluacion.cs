using AcademicPlanner.Domain.Common;
using AcademicPlanner.Domain.Enums;

namespace AcademicPlanner.Domain.Entities;

public class Evaluacion : BaseEntity
{
    public Guid MateriaId { get; private set; }
    public Materia Materia { get; private set; } = null!;
    public TipoEvaluacion Tipo { get; private set; }
    public EstadoEvaluacion Estado { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal? Nota { get; private set; }
    public string? Descripcion { get; private set; }

    protected Evaluacion() { }

    public static Evaluacion Crear(Guid materiaId, TipoEvaluacion tipo, DateOnly fecha, string? descripcion = null)
    {
        if (materiaId == Guid.Empty)
            throw new ArgumentException("El Id de la materia no puede estar vacío.", nameof(materiaId));

        return new Evaluacion
        {
            MateriaId = materiaId,
            Tipo = tipo,
            Estado = EstadoEvaluacion.Pendiente,
            Fecha = fecha,
            Descripcion = descripcion?.Trim()
        };
    }

    public void RegistrarNota(decimal nota)
    {
        if (nota < 1 || nota > 10)
            throw new ArgumentOutOfRangeException(nameof(nota), "La nota debe estar entre 1 y 10.");

        Nota = nota;
    }
}
