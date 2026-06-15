using AcademicPlanner.Domain.Common;

namespace AcademicPlanner.Domain.Entities;

public class HorarioCursada : BaseEntity
{
    public Guid MateriaId { get; private set; }
    public Materia Materia { get; private set; } = null!;
    public DayOfWeek DiaSemana { get; private set; }
    public TimeOnly HoraInicio { get; private set; }
    public TimeOnly HoraFin { get; private set; }
    public string? Aula { get; private set; }
    public bool EsVirtual { get; private set; }

    protected HorarioCursada() { }

    public static HorarioCursada Crear(
        Guid materiaId,
        DayOfWeek diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        string? aula = null,
        bool esVirtual = false)
    {
        if (materiaId == Guid.Empty)
            throw new ArgumentException("El Id de la materia no puede estar vacío.", nameof(materiaId));

        if (horaFin <= horaInicio)
            throw new ArgumentException("La hora de fin debe ser posterior a la hora de inicio.");

        return new HorarioCursada
        {
            MateriaId = materiaId,
            DiaSemana = diaSemana,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            Aula = aula?.Trim(),
            EsVirtual = esVirtual
        };
    }

    public void Actualizar(
        DayOfWeek diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        string? aula,
        bool esVirtual)
    {
        if (horaFin <= horaInicio)
            throw new ArgumentException("La hora de fin debe ser posterior a la hora de inicio.");

        DiaSemana = diaSemana;
        HoraInicio = horaInicio;
        HoraFin = horaFin;
        Aula = aula?.Trim();
        EsVirtual = esVirtual;
    }
}
