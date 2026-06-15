using AcademicPlanner.Application.Features.Agenda.Commands.ActualizarHorario;
using FluentAssertions;
using Xunit;

namespace AcademicPlanner.Tests.Features.Agenda;

public sealed class ActualizarHorarioValidatorTests
{
    private readonly ActualizarHorarioValidator _sut = new();

    [Fact]
    public void Validar_MateriaIdVacio_RetornaError()
    {
        var command = Valido() with { MateriaId = Guid.Empty };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MateriaId));
    }

    [Fact]
    public void Validar_HorarioIdVacio_RetornaError()
    {
        var command = Valido() with { HorarioId = Guid.Empty };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.HorarioId));
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

    private static ActualizarHorarioCommand Valido() => new(
        Guid.NewGuid(), Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(18, 30), new TimeOnly(21, 30), "Aula 5", false);
}
