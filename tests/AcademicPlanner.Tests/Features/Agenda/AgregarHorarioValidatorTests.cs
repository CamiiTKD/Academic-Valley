using AcademicPlanner.Application.Features.Agenda.Commands.AgregarHorario;
using AcademicPlanner.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AcademicPlanner.Tests.Features.Agenda;

public sealed class AgregarHorarioValidatorTests
{
    private readonly AgregarHorarioValidator _sut = new();

    [Fact]
    public void Validar_MateriaIdVacio_RetornaError()
    {
        var command = Valido() with { MateriaId = Guid.Empty };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MateriaId));
    }

    [Fact]
    public void Validar_HoraFinAnteriorAHoraInicio_RetornaError()
    {
        var command = Valido() with { HoraInicio = new TimeOnly(20, 0), HoraFin = new TimeOnly(18, 0) };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("hora de fin"));
    }

    [Fact]
    public void Validar_HoraFinIgualAHoraInicio_RetornaError()
    {
        var hora = new TimeOnly(18, 0);
        var command = Valido() with { HoraInicio = hora, HoraFin = hora };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validar_AulaExcede100Caracteres_RetornaError()
    {
        var command = Valido() with { Aula = new string('A', 101) };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Aula));
    }

    [Fact]
    public void Validar_DatosValidos_SinErrores()
    {
        _sut.Validate(Valido()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_AulaNula_SinErrores()
    {
        _sut.Validate(Valido() with { Aula = null }).IsValid.Should().BeTrue();
    }

    // ─── Reglas de dominio de HorarioCursada ────────────────────────────────

    [Fact]
    public void Crear_MateriaIdVacio_LanzaArgumentException()
    {
        var act = () => HorarioCursada.Crear(Guid.Empty, DayOfWeek.Monday, new TimeOnly(18, 0), new TimeOnly(21, 0));
        act.Should().Throw<ArgumentException>().WithMessage("*materiaId*");
    }

    [Fact]
    public void Crear_HoraFinAnteriorAHoraInicio_LanzaArgumentException()
    {
        var act = () => HorarioCursada.Crear(Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(20, 0), new TimeOnly(18, 0));
        act.Should().Throw<ArgumentException>().WithMessage("*posterior*");
    }

    [Fact]
    public void Actualizar_HoraFinAnteriorAHoraInicio_LanzaArgumentException()
    {
        var horario = HorarioCursada.Crear(Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0));
        var act = () => horario.Actualizar(DayOfWeek.Wednesday, new TimeOnly(20, 0), new TimeOnly(18, 0), null, false);
        act.Should().Throw<ArgumentException>().WithMessage("*posterior*");
    }

    private static AgregarHorarioCommand Valido() => new(
        Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(18, 30), new TimeOnly(21, 30), "Aula 5", false);
}
