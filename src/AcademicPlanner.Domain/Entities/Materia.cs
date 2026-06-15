using AcademicPlanner.Domain.Common;
using AcademicPlanner.Domain.Enums;

namespace AcademicPlanner.Domain.Entities;

public class Materia : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public int Cuatrimestre { get; private set; }
    public decimal? NotaFinal { get; private set; }
    public EstadoMateria Estado { get; private set; }

    // Self-referencing M:M — materias que se deben aprobar antes de cursar esta
    public ICollection<Materia> Correlativas { get; private set; } = [];

    public ICollection<Evaluacion> Evaluaciones { get; private set; } = [];

    public ICollection<HorarioCursada> Horarios { get; private set; } = [];

    // Parameterless constructor required by EF Core
    protected Materia() { }

    public static Materia Crear(string nombre, string codigo, int cuatrimestre)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

        if (cuatrimestre < 1)
            throw new ArgumentOutOfRangeException(nameof(cuatrimestre), "El cuatrimestre debe ser mayor a 0.");

        return new Materia
        {
            Nombre = nombre.Trim(),
            Codigo = codigo.Trim().ToUpperInvariant(),
            Cuatrimestre = cuatrimestre,
            Estado = EstadoMateria.Pendiente
        };
    }

    public void ActualizarEstado(EstadoMateria nuevoEstado, decimal? nota = null)
    {
        Estado = nuevoEstado;
        if (nota.HasValue)
            NotaFinal = nota;
    }

    public void AgregarCorrelativa(Materia correlativa)
    {
        ArgumentNullException.ThrowIfNull(correlativa);

        if (correlativa.Id == Id)
            throw new InvalidOperationException("Una materia no puede ser correlativa de sí misma.");

        Correlativas.Add(correlativa);
    }
}
