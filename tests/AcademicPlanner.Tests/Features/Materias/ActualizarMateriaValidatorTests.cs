using AcademicPlanner.Application.Features.Materias.Commands.ActualizarMateria;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AcademicPlanner.Tests.Features.Materias;

public sealed class ActualizarMateriaValidatorTests
{
    private readonly ActualizarMateriaValidator _sut = new();

    [Fact]
    public void Validar_IdVacio_RetornaError()
    {
        var command = Valido() with { Id = Guid.Empty };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
    }

    [Fact]
    public void Validar_NombreVacio_RetornaError()
    {
        var command = Valido() with { Nombre = "" };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Nombre));
    }

    [Fact]
    public void Validar_NombreExcede200Caracteres_RetornaError()
    {
        var command = Valido() with { Nombre = new string('N', 201) };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Nombre));
    }

    [Fact]
    public void Validar_CodigoVacio_RetornaError()
    {
        var command = Valido() with { Codigo = "" };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Codigo));
    }

    [Fact]
    public void Validar_CodigoExcede20Caracteres_RetornaError()
    {
        var command = Valido() with { Codigo = new string('C', 21) };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Codigo));
    }

    [Fact]
    public void Validar_CuatrimestreCero_RetornaError()
    {
        var command = Valido() with { Cuatrimestre = 0 };
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Cuatrimestre));
    }

    [Fact]
    public void Validar_DatosValidos_SinErrores()
    {
        _sut.Validate(Valido()).IsValid.Should().BeTrue();
    }

    private static ActualizarMateriaCommand Valido() =>
        new(Guid.NewGuid(), "Bases de Datos", "BD1", 3, EstadoMateria.Cursando);
}
